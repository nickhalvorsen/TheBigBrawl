using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.CharacterController
{
    [RequireComponent(typeof(AudioSync))]
    [RequireComponent(typeof(ParticleSync))]
    [RequireComponent(typeof(ForceSync))]
    public class vThirdPersonController : vThirdPersonAnimator
    {
        private Animator _animator;
        private AudioSync _audioSync;
        private ParticleSync _particleSync;
        private ForceSync _forceSync;
        private PlayerGameRules _playerGameRules;
        private PlayerName _playerName;

        private const float slapDuration = 0.40f;
        private const float slapEffectDelay = 0.15f;
        private float slapTimer = 0; // slaptimer starts at slapduration and counts down to 0
        // whether the effect happened or not already
        // during this animation 
        private bool slapEffectHappened = false;
        public float DamagePercent;
        private enum AttackState { Attacking, Waiting }
        private AttackState attackState = AttackState.Waiting;

        protected virtual void Start()
        {
            _animator = GetComponent<Animator>();
            _audioSync = GetComponent<AudioSync>();
            _particleSync = GetComponent<ParticleSync>();
            _forceSync = GetComponent<ForceSync>();
            _playerGameRules = GetComponent<PlayerGameRules>();
            _playerName = GetComponent<PlayerName>();
            // Instead of syncing the name upon connecting, just do this
            InvokeRepeating("SyncName", 1f, 5f);

#if !UNITY_EDITOR
                Cursor.visible = false;
#endif
        }

        private void SyncName()
        {
            if (isLocalPlayer)
            {
                GetComponent<PlayerName>().UpdateName();
            }            
        }

        public virtual void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            switch (attackState)
            {
                case AttackState.Waiting:
                    UpdateWaitingState();
                    break;
                case AttackState.Attacking:
                    UpdateAttackState();
                    break;
            }

            CheckIfOutOfBounds();
        }

        void UpdateWaitingState()
        {
            if (Input.GetMouseButtonDown(0))
            {
                BeginSlapping();
            }
        }

        void UpdateAttackState()
        {
            slapTimer -= Time.deltaTime;

            if (!slapEffectHappened && slapTimer < slapDuration - slapEffectDelay)
            {
                // The slap animation has progressed 
                // to the point where the explosion should be generated now
                TriggerSlapEffect();
                slapEffectHappened = true;
            }

            if (slapTimer < 0)
            {
                // The slap animation is complete 
                // and we are no longer slapping
                attackState = AttackState.Waiting;
                _animator.ResetTrigger("attack");
            }
        }

        void BeginSlapping()
        {
            _animator.SetTrigger("attack");
            slapTimer = slapDuration;
            attackState = AttackState.Attacking;
            slapEffectHappened = false;
        }

        void TriggerSlapEffect()
        {
            TriggerSlapParticle();
            TriggerSlapForce();
            TriggerSlapSound();
        }

        void TriggerSlapParticle()
        {
            _particleSync.PlayParticle();
        }

        void TriggerSlapForce()
        {
            _forceSync.PlayForce();
        }

        void TriggerSlapSound()
        {
            _audioSync.PlayWorldSound(Sounds.Slap);
        }

        void CheckIfOutOfBounds()
        {
            if (this.transform.position.y < -10)
            {
                Respawn();
            }
        }

        void Respawn()
        {
            // respawn point
            this.transform.position = new Vector3(0, 70, 0);
            this._rigidbody.velocity = new Vector3(0, 0, 0);
            _audioSync.PlayWorldSound(Sounds.Death);
            _playerGameRules.Died();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (collision?.collider?.name == null)
            {
                return;
            }

            if (collision.collider.name.StartsWith("arena"))
            {
                _playerGameRules.EnteredArena();
            }

            // this object is for testing
            if (collision.collider.name == "Bouncer")
            {
                this.gameObject.GetComponent<Rigidbody>().AddExplosionForce(200, this.gameObject.transform.position + new Vector3(0,0,0), 10, 0.5f, ForceMode.Impulse);
            }

            if (collision.collider.gameObject.tag == "Moving platform")
            {
                //this.transform.parent = collision.collider.gameObject.transform;
            }

            // todo delete gems if they are in water 
            // todo clap affects gems 
            if (collision.collider.gameObject.tag == "Pickup_Money")
            {
                _playerGameRules.PickedUpMoney();
                _audioSync.PlayWorldSound(Sounds.PickupMoney);
                StartCoroutine(RemovePickupAfterDelay(collision.collider.gameObject));
            }
        }

        private IEnumerator RemovePickupAfterDelay(GameObject pickupGameObject)
        {     
            yield return new WaitForSeconds(0.05f);
            pickupGameObject.SetActive(false);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.gameObject.tag == "Moving platform")
            {
                //this.transform.parent = null;
            }
        }

        void OnCollisionStay(Collision other)
        {
            if (other.transform.tag == "Moving platform")
            {
                GetComponent<Rigidbody>().velocity
                        = other.gameObject.GetComponent<Rigidbody>().velocity;
            }
        }

        public virtual void Sprint(bool value)
        {                                   
            isSprinting = value;            
        }

        public virtual void Strafe()
        {
            if (locomotionType == LocomotionType.OnlyFree) return;
            isStrafing = !isStrafing;
        }

        public virtual void Jump()
        {
            // conditions to do this action
            bool jumpConditions = isGrounded && !isJumping;
            // return if jumpCondigions is false
            if (!jumpConditions) return;
            // trigger jump behaviour
            jumpCounter = jumpTimer;            
            isJumping = true;
            // trigger jump animations            
            if (_rigidbody.velocity.magnitude < 1)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", 0.2f);
        }

        public virtual void RotateWithAnotherTransform(Transform referenceTransform)
        {
            var newRotation = new Vector3(transform.eulerAngles.x, referenceTransform.eulerAngles.y, transform.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), strafeRotationSpeed * Time.fixedDeltaTime);
            targetRotation = transform.rotation;
        }

        public static class Sounds
        {
            public const int Slap = 0;
            public const int Death = 1;
            public const int EnterArena = 2;
            public const int PickupMoney = 3;
        }
    }
}
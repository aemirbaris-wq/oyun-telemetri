using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            
            m_MoveAction.Enable();
            m_JumpAction.Enable();
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = m_MoveAction.ReadValue<Vector2>().x;
                GuncelleZiplaTamponu();
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            GuncelleZiplaTamponu();
            base.Update();
        }

        private float sonYerdeOlmaZamani = -10f;
        private float sonZiplaIstekZamani = -10f;
        private bool asagiHizliInisAktif = false;

        private void GuncelleZiplaTamponu()
        {
            if (IsGrounded || jumpState == JumpState.Grounded)
            {
                sonYerdeOlmaZamani = Time.time;
            }

            if (Time.time - sonZiplaIstekZamani <= 0.13f)
            {
                bool coyoteAktif = (Time.time - sonYerdeOlmaZamani) <= 0.11f;
                if (jumpState == JumpState.Grounded || (IsGrounded && jumpState == JumpState.Landed) || (coyoteAktif && velocity.y <= 0.05f && jumpState != JumpState.Jumping && jumpState != JumpState.InFlight))
                {
                    sonZiplaIstekZamani = -10f;
                    sonYerdeOlmaZamani = -10f;
                    jumpState = JumpState.PrepareToJump;
                    stopJump = false;
                }
            }
        }

        public void TetikleZiplaHarici()
        {
            sonZiplaIstekZamani = Time.time;
            GuncelleZiplaTamponu();
        }

        public void DurdurZiplaHarici()
        {
            if (!IsGrounded && velocity.y > 0f)
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }

        public void UygulaAsagiOkEtkisi(bool asagiBasili)
        {
            asagiHizliInisAktif = (!IsGrounded && asagiBasili);
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                float mod = model != null ? model.jumpModifier : 1f;
                velocity.y = jumpTakeOffSpeed * mod;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0f)
                {
                    float decel = model != null ? model.jumpDeceleration : 0.5f;
                    velocity.y = velocity.y * decel;
                }
            }

            // Dogal ve tok inis yercekimi (havada asili kalmayi onler, animasyonu bozmaz)
            if (!IsGrounded && velocity.y < 0f)
            {
                float ekYercekimi = asagiHizliInisAktif ? 1.15f : 0.55f;
                float maksDususHizi = asagiHizliInisAktif ? -15f : -12f;
                velocity.y += Physics2D.gravity.y * ekYercekimi * Time.deltaTime;
                if (velocity.y < maksDususHizi)
                {
                    velocity.y = maksDususHizi;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            // Havadayken sag/sol ok basiliysa bosluklari dash'siz gecebilmek icin akici yatay kavis
            float yatayCarpan = (!IsGrounded && Mathf.Abs(move.x) > 0.1f) ? 1.28f : 1f;
            targetVelocity = new Vector2(move.x * maxSpeed * yatayCarpan, 0f);
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}


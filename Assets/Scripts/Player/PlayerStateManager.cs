using System.Collections.Generic;
using Unity;
using UnityEngine;

namespace TrianCatStudio
{
    public class PlayerStateManager : MonoBehaviour
    {
        public StateMachine StateMachine { get; private set; }
        public Player Player { get; private set; }

        // ״̬ʵ��
        public IdleState IdleState { get; private set; }
        public WalkState WalkState { get; private set; }
        public RunState RunState { get; private set; }
        public JumpState JumpState { get; private set; }
        public RollState RollState { get; private set; }

        private void Awake()
        {
            Init(); 
        }

        protected void Init()
        {
            StateMachine = new StateMachine();
            Player = FindObjectOfType<Player>();
            

            InitializeStates();
            RegisterTransitions();
            SetInitialState();
        }

        private void InitializeStates()
        {
            IdleState = new IdleState(this);
            WalkState = new WalkState(this);
            RunState = new RunState(this);
            JumpState = new JumpState(this);
            RollState = new RollState(this);
        }

        private void Update()
        {
            SyncStateParameters();
            HandleGlobalTransitions();
        }

        private void SyncStateParameters()
        {
            // ͬ���������
            StateMachine.SetFloat("MoveInput", Player.InputManager.GetInputMagnitude());
            StateMachine.SetBool("IsSprinting", Player.InputManager.IsRunning);

            // ͬ����ɫ״̬
            StateMachine.SetBool("IsGrounded", Player.IsGrounded);
            StateMachine.SetFloat("VerticalVelocity", Player.Velocity.y);
        }

        private void RegisterTransitions()
        {
            // �����ƶ�״̬ת��
            AddTransition(IdleState, WalkState,
                new Condition("MoveInput", ParameterType.Float, ComparisonType.GreaterThan, 0.1f));

            AddTransition(WalkState, IdleState,
                new Condition("MoveInput", ParameterType.Float, ComparisonType.LessOrEqual, 0.1f));

            AddTransition(WalkState, RunState,
                new Condition("IsSprinting", ParameterType.Bool, ComparisonType.Equals, true),
                new Condition("Stamina", ParameterType.Float, ComparisonType.GreaterThan, 0f));

            AddTransition(RunState, WalkState,
                new Condition("IsSprinting", ParameterType.Bool, ComparisonType.Equals, false));

            // ��Ծ״̬ת��
            AddGlobalTransition(JumpState,
                new Condition("Jump", ParameterType.Trigger, ComparisonType.Equals, true),
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true));

            // ����ȫ���ж�
            AddGlobalTransition(RollState,
                new Condition("Roll", ParameterType.Trigger, ComparisonType.Equals, true),
                new Condition("IsGrounded", ParameterType.Bool, ComparisonType.Equals, true));
        }

        private void HandleGlobalTransitions()
        {

        }

        private void AddTransition(IState from, IState to, params Condition[] conditions)
        {
            var transition = new Transition
            {
                TargetState = to,
                Conditions = new List<Condition>(conditions)
            };
            StateMachine.AddTransition(from, transition);
        }

        private void AddGlobalTransition(IState to, params Condition[] conditions)
        {
            foreach (var state in new IState[] { IdleState, WalkState, RunState })
            {
                AddTransition(state, to, conditions);
            }
        }

        private void SetInitialState() => StateMachine.SetCurrentState(IdleState);

        // �����¼��ӿ�
        public void TriggerJump() => StateMachine.SetTrigger("Jump");
        public void TriggerRoll() => StateMachine.SetTrigger("Roll");
    }
}
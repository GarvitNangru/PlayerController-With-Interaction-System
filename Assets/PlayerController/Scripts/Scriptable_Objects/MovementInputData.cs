using UnityEngine;

namespace PlayerController
{    
    [CreateAssetMenu(fileName = "MovementInputData", menuName = "FirstPersonController/Data/MovementInputData", order = 1)]
    public class MovementInputData : ScriptableObject
    {
        #region Data
            Vector2 m_inputVector;

            bool m_isRunning;
            bool m_isCrouching;

            bool m_crouchClicked;
            bool m_jumpClicked;
        #endregion

        #region Properties
            public Vector2 InputVector => m_inputVector;
            public bool HasInput => m_inputVector != Vector2.zero;
            public float InputVectorX
            {
                set => m_inputVector.x = value;
            }

            public float InputVectorY
            {
                set => m_inputVector.y = value;
            }

            public bool IsRunning
            {
                get => m_isRunning;
                set => m_isRunning = value;
            }

            public bool IsCrouching
            {
                get => m_isCrouching;
                set => m_isCrouching = value;
            }

            public bool CrouchClicked
            {
                get => m_crouchClicked;
                set => m_crouchClicked = value;
            }

            public bool JumpClicked
            {
                get => m_jumpClicked;
                set => m_jumpClicked = value;
            }
        #endregion

        #region Custom Methods
            public void ResetInput()
            {
                m_inputVector = Vector2.zero;
                
                m_isRunning = false;
                m_isCrouching = false;

                m_crouchClicked = false;
                m_jumpClicked = false;
            }
        #endregion
    }
}
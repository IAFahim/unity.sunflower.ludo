using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElProfesorKudo.Firebase.PopUp
{
    using ElProfesorKudo.Firebase.Common;
    public class PopUpObject : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [SerializeField] private TextMeshProUGUI _validateText;
        [SerializeField] private TextMeshProUGUI _cancelText;
        [SerializeField] private TextMeshProUGUI _okText;

        [SerializeField] private Button _validateButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _okButton;

        [SerializeField] private Animator _animatorPopUp;

        private Callback _onValidate;
        private Callback _onCancel;
        private Callback _onOk;

        public enum PopUpType
        {
            POP_UP_TYPE_T_B_V_C, // Title body Validate cancel
            POP_UP_TYPE_T_B_O, // Title body ok
            POP_UP_TYPE_T_V_C, // Title  Validate cancel
            POP_UP_TYPE_T_O, // Title  ok
            POP_UP_TYPE_B_V_C, // body Validate cancel
            POP_UP_TYPE_B_O, //  body ok
        }

        [HideInInspector]
        public PopUpType _popUpType;

        public void ShowUpType(PopUpType type)
        {
            _popUpType = type;
            {
                switch (type)
                {
                    case PopUpType.POP_UP_TYPE_T_B_V_C:
                        _title.gameObject.SetActive(true);
                        _bodyText.gameObject.SetActive(true);
                        _validateButton.gameObject.SetActive(true);
                        _cancelButton.gameObject.SetActive(true);
                        _okButton.gameObject.SetActive(false);
                        break;
                    case PopUpType.POP_UP_TYPE_T_B_O:
                        _title.gameObject.SetActive(true);
                        _bodyText.gameObject.SetActive(true);
                        _validateButton.gameObject.SetActive(false);
                        _cancelButton.gameObject.SetActive(false);
                        _okButton.gameObject.SetActive(true);
                        break;
                    case PopUpType.POP_UP_TYPE_T_V_C:
                        _title.gameObject.SetActive(true);
                        _bodyText.gameObject.SetActive(false);
                        _validateButton.gameObject.SetActive(true);
                        _cancelButton.gameObject.SetActive(true);
                        _okButton.gameObject.SetActive(false);
                        break;
                    case PopUpType.POP_UP_TYPE_T_O:
                        _title.gameObject.SetActive(true);
                        _bodyText.gameObject.SetActive(false);
                        _validateButton.gameObject.SetActive(false);
                        _cancelButton.gameObject.SetActive(false);
                        _okButton.gameObject.SetActive(true);
                        break;
                    case PopUpType.POP_UP_TYPE_B_V_C:
                        _title.gameObject.SetActive(false);
                        _bodyText.gameObject.SetActive(true);
                        _validateButton.gameObject.SetActive(true);
                        _cancelButton.gameObject.SetActive(true);
                        _okButton.gameObject.SetActive(false);
                        break;
                    case PopUpType.POP_UP_TYPE_B_O:
                        _title.gameObject.SetActive(false);
                        _bodyText.gameObject.SetActive(true);
                        _validateButton.gameObject.SetActive(false);
                        _cancelButton.gameObject.SetActive(false);
                        _okButton.gameObject.SetActive(true);
                        break;
                    default:
                        break;
                }
            }
        }

        public void InitPopUp(PopUpType type = PopUpType.POP_UP_TYPE_T_B_V_C,
                                string titleText = "my Title", string bodyText = "My body text", string validateText = "Validate",
                                string cancelText = "Cancel", string okText = "Ok",
                                Callback validateButton = null, Callback cancelButton = null, Callback okButton = null)
        {
            _title.text = titleText;
            _bodyText.text = bodyText;
            _validateText.text = validateText;
            _cancelText.text = cancelText;
            _okText.text = okText;
            _onValidate = validateButton;
            _onCancel = cancelButton;
            _onOk = okButton;
            ShowUpType(type);
        }


        public void OnClickValidate()
        {
            if (_onValidate != null)
            {
                _onValidate();
            }
        }
        public void OnClickCancel()
        {
            if (_onCancel != null)
            {
                _onCancel();
            }
        }
        public void OnClickOk()
        {
            if (_onOk != null)
            {
                _onOk();
            }
        }

        public void ClosePopUp()
        {
            _animatorPopUp.Play("ClosePopUp");
        }

        public void DestroyPopUp()
        {
            Destroy(gameObject);
        }

    }
}

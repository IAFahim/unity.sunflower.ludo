using UnityEngine;

namespace ElProfesorKudo.Firebase.PopUp
{
    using ElProfesorKudo.Firebase.Common;
    public class PopUpManager : Singleton<PopUpManager>
    {
        [SerializeField] private GameObject _prefabPopUpObject;
        [SerializeField] private GameObject _mainCanvas;

        private PopUpObject _popUpObject;

        [HideInInspector] public const string defaultTextValidate = "Validate";
        [HideInInspector] public const string defaultTextCancel = "Cancel";
        [HideInInspector] public const string defaultTextOk = "Ok";

        public void InstancePopUp(PopUpObject.PopUpType type = PopUpObject.PopUpType.POP_UP_TYPE_T_B_V_C,
                                 string titleText = "my Title", string bodyText = "My body text", string validateText = defaultTextValidate,
                                 string cancelText = defaultTextCancel, string okText = defaultTextOk,
                                 Callback validateButton = null, Callback cancelButton = null, Callback okButton = null)
        {
            if (_popUpObject == null)
            {
                _popUpObject = Instantiate(_prefabPopUpObject, _mainCanvas.transform).GetComponent<PopUpObject>();
                _popUpObject.InitPopUp(type, titleText, bodyText, validateText, cancelText, okText,
                                        validateButton, cancelButton, okButton);
            }
        }

        public void ForceClosePopUp()
        {
            if (_popUpObject != null)
            {
                _popUpObject.ClosePopUp();
            }
        }
    }
}

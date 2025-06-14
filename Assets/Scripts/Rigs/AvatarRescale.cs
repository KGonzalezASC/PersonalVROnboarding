using UnityEngine;
using UnityEngine.InputSystem;

//Currently, uses character controller's height to rescale avatar but it might lead to issues
//as it is set to 1.36 most times?
public class AvatarRescale : MonoBehaviour
{
    public InputActionReference resizeAction;
    [SerializeField] private CharacterController characterController;
    private float defaultHeight = 1.78f;

    void OnEnable()
    {
        if (resizeAction != null)
            resizeAction.action.performed += ResizeAvatar;
    }

    void OnDisable()
    {
        if (resizeAction != null)
            resizeAction.action.performed -= ResizeAvatar;
    }

    void ResizeAvatar(InputAction.CallbackContext ctx)
    {
        float heightScale = characterController.height / defaultHeight;
        transform.localScale = Vector3.one * heightScale;
    }
}

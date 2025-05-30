using System;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables
{
    // Ensure a LineRenderer and Collider are present for visuals and interaction.
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(Collider))]
    public class XRCustomPullInteractable : XRBaseInteractable
    {
        // Fired when the user begins pulling (grab enters).
        public event Action PullActionStarted;
        // Fired each frame the pull amount changes (value in [0…1]).
        public event Action<float> PullUpdated;
        // Fired once on release, providing the final pullAmount.
        public event Action<float> PullActionReleased;
        // Fired once on release, after PullActionReleased.
        public event Action PullActionEnded;

        [Header("Pull Configuration")]
        [SerializeField] private Transform _startPoint;        // Local start position
        [SerializeField] private Transform _endPoint;          // Local fully-pulled position
        [SerializeField] private GameObject _pullPoint;        // Visual element that moves

        [Tooltip("If true, the pullPoint snaps back to start on release; otherwise it stays where released.")]
        [SerializeField] private bool _returnToStartOnRelease = true;

        private LineRenderer _lineRenderer;                    // Draws line to pullPoint
        private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor _pullInteractor = null;    // Who is currently pulling
        private float _pullAmount = 0.0f;                      // Normalized pull (0=start, 1=end)

        /// <summary>
        /// Read-only access to current pull amount.
        /// </summary>
        public float PullAmount => _pullAmount;
        
        [Header("Dependencies")]
        [Tooltip("The XRGrabInteractable on the bow. Pulling only works while this is held.")]
        [SerializeField] private XRGrabInteractable _bowGrabInteractable;

        protected override void Awake()
        {
            base.Awake();
            // Cache LineRenderer (required by RequireComponent).
            _lineRenderer = GetComponent<LineRenderer>();
            
            
            // subscribe to bow grab/release
            if (_bowGrabInteractable != null)
            {
                _bowGrabInteractable.selectEntered.AddListener(OnBowGrabbed);
                _bowGrabInteractable.selectExited .AddListener(OnBowReleased);
            }

            // start with string disabled
            SetStringInteractable(false);
        }
        
        
        private void OnBowGrabbed(SelectEnterEventArgs _)
        {
            // Enable the string interactable and its collider so you can hover & select it
            SetStringInteractable(true);
        }

        private void OnBowReleased(SelectExitEventArgs _)
        {
            // If the string is currently held, force-release it
            if (isSelected)
                interactionManager.CancelInteractableSelection((IXRSelectInteractable)this);

            // Disable the string again
            SetStringInteractable(false);
        }


        /// <summary>
        /// Enables/disables this component and its Collider(s),
        /// which controls whether near/far interactors can hover or select it.
        /// </summary>
        private void SetStringInteractable(bool on)
        {
            // enable/disable the behaviour itself
            enabled = on;

            // toggle all colliders (including trigger) on this GameObject
            foreach (var c in GetComponents<Collider>())
                c.enabled = on;
        }

        public void SetInteractor(SelectEnterEventArgs arg)
        {
            _pullInteractor = arg.interactorObject;
            PullActionStarted?.Invoke();
        }

        public void Release()
        {
            if (_returnToStartOnRelease)
            {
                PullActionReleased?.Invoke(_pullAmount);   // Notify that value reset
                _pullAmount = 0f;
                UpdatePullVisuals();             
            }
            PullActionEnded?.Invoke();
            _pullInteractor = null;
            var pos = _pullPoint.transform.position;
            _pullPoint.transform.localPosition =
                new Vector3(pos.x, pos.y, 0f);
            UpdatePullVisuals();
        }

        
        /// <summary>
        /// Called when an interactor grabs this object.
        /// </summary>
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            SetInteractor(args);
        }


        /// <summary>
        /// Called by XR Interaction system each frame for this interactable.
        /// We use the Dynamic phase to update pull logic.
        /// </summary>
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            // 1) Only run during the Dynamic update phase
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                return;

            // 2) Require that the bow itself is currently grabbed by some interactor
            if (!_bowGrabInteractable || !_bowGrabInteractable.isSelected)
                return;

            // 3) Then require that this pull piece is selected and we have a pulling interactor
            if (!isSelected || _pullInteractor == null)
                return;

            // ——— your existing pull logic ———

            // Determine the world-space attach point of the interactor.
            Vector3 worldPullPos = _pullInteractor.GetAttachTransform(this).position;
            // Calculate new normalized pull amount.
            float newPull = CalcPull(worldPullPos);

            // If changed, update and fire event.
            if (!Mathf.Approximately(newPull, _pullAmount))
            {
                _pullAmount = newPull;
                PullUpdated?.Invoke(_pullAmount);
            }

            // Update visual position of pullPoint and line.
            UpdatePullVisuals();
        }


        /// <summary>
        /// Projects the given world pull position onto the line defined by _startPoint→_endPoint.
        /// Returns a value in [0…1] indicating relative pull.
        /// </summary>
        private float CalcPull(Vector3 worldPullPos)
        {
            Vector3 startWS = _startPoint.position;
            Vector3 endWS = _endPoint.position;
            Vector3 axis = endWS - startWS;
            float maxLength = axis.magnitude;
            if (maxLength < Mathf.Epsilon)
                return 0f;

            Vector3 dir = axis.normalized;
            // Compute distance from start along axis.
            float dist = Vector3.Dot(worldPullPos - startWS, dir);
            // Clamp between 0 and maxLength.
            float clamped = Mathf.Clamp(dist, 0f, maxLength);
            // Normalize to [0…1].
            return clamped / maxLength;
        }

        /// <summary>
        /// Moves the visual pullPoint and updates the LineRenderer
        /// based on the current _pullAmount.
        /// </summary>
        private void UpdatePullVisuals()
        {
            // Interpolate in local space between start and end.
            Vector3 localStart = _startPoint.localPosition;
            Vector3 localEnd   = _endPoint.localPosition;
            Vector3 newLocal   = Vector3.Lerp(localStart, localEnd, _pullAmount);

            // Move the pull point GameObject.
            _pullPoint.transform.localPosition = newLocal;

            // Draw the line from origin (index 0) to pullPoint (index 1).
            _lineRenderer.SetPosition(1, newLocal);
        }
    }
}

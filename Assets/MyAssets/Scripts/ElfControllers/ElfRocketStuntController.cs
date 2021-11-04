using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyAssets.Scripts.ElfControllers
{
    public class ElfRocketStuntController : MonoBehaviour
    {
        [SerializeField] private Transform ElfTransform;
        [SerializeField] private float TimeToClimb = 5f;
        [SerializeField] private float DistanceToClimb = 40f;
        [SerializeField] private float TimeToRally = 4f;
        [SerializeField] private float DistanceToSprint = 30f;
        [SerializeField] private float TimeToSpint = 3f;
        [SerializeField] private float DistanceToFly = 200f;
        [SerializeField] private float TimeToFly = 3f;
        [SerializeField] private float DistanceToCurveVertical = 50f;
        [SerializeField] private float DistanceToCurveHorizontal = 50f;
        [SerializeField] private float TimeToCurve = 2f;
        [SerializeField] private float TimeToFlyRotZ = .5f;
        [SerializeField] private AnimationCurve CurveHorizontalEase;
        [SerializeField] private AnimationCurve CurveVerticalEase;
        [SerializeField] private AnimationCurve CurveRotXEase;
        [SerializeField] private AnimationCurve FlightRotZEase;
        [SerializeField] private float DistanceToFlyReturn = 200f;
        [SerializeField] private float TimeToFlyReturn = 3f;
        [SerializeField] private float TimeToFall = 2f;
        [SerializeField] private float RotateTime = 2f;
        [SerializeField] private float CheeringTime = 4f;
        
        public UnityEvent OnReset;
        public UnityEvent OnFinishClimb;
        public UnityEvent OnFinishRally;
        public UnityEvent OnFinishSprint;
        public UnityEvent OnFinishFlying;
        public UnityEvent OnFinishFalling;
        public UnityEvent OnFinishCheering;

        private LTDescr ClimpTween;
        private LTDescr RallyTween;
        private LTDescr SprintTween;
        private LTDescr FlyTween;
        private LTDescr CurveTweenHorizontal;
        private LTDescr CurveTweenVertical;
        private LTDescr CurveTweenRotationX;
        private LTDescr CurveTweenRotationZ;
        private LTDescr ReturnFlyTween;
        private LTDescr FallingTween;
        private LTDescr RotateTween;
        private LTDescr CheeringTween;
        private Vector3 StartPos;
        private Vector3 TopOfLadderPos;

        private void Awake()
        {
            StartPos = ElfTransform.position;
        }
        
        private void OnDisable()
        {
            Reset();
        }
        public void Activate()
        {
            StartClimbLadderTween();
        }

        public void Reset()
        {
            if (ClimpTween != null) LeanTween.cancel(ClimpTween.id);
            if (RallyTween != null) LeanTween.cancel(RallyTween.id);
            if (SprintTween != null)   LeanTween.cancel(SprintTween.id);
            if (FlyTween != null)   LeanTween.cancel(FlyTween.id);
            if (CurveTweenHorizontal != null)   LeanTween.cancel(CurveTweenHorizontal.id);
            if (CurveTweenVertical != null)   LeanTween.cancel(CurveTweenVertical.id);
            if (CurveTweenRotationX != null)   LeanTween.cancel(CurveTweenRotationX.id);
            if (CurveTweenRotationZ != null)   LeanTween.cancel(CurveTweenRotationZ.id);
            if (ReturnFlyTween != null)   LeanTween.cancel(ReturnFlyTween.id);
            if (FallingTween != null) LeanTween.cancel(FallingTween.id);
            if (RotateTween != null) LeanTween.cancel(RotateTween.id);
            if (CheeringTween != null) LeanTween.cancel(CheeringTween.id);
            ElfTransform.rotation = Quaternion.identity;
            OnReset.Invoke();
        }

        private void StartClimbLadderTween()
        {
            TopOfLadderPos = StartPos + Vector3.up * DistanceToClimb;
            
            //Climb up ladder
            ClimpTween = LeanTween.value(0f, 1f,  TimeToClimb);
            ClimpTween.setOnUpdate(t =>
            {
                ElfTransform.position = Vector3.Lerp(StartPos, TopOfLadderPos, t);
            });
            ClimpTween.setOnComplete(_ =>
            {
                OnFinishClimb.Invoke();
                StartRallyTween();
            });
        }
        
        private void StartRallyTween() {
            RallyTween = LeanTween.value(0f, 1f,  TimeToRally);
            RallyTween.setOnComplete(_ =>
            {
                OnFinishRally.Invoke();
                StartSprintTween();
            });
        }

        private void StartSprintTween()
        {
            Vector3 startPos = ElfTransform.position;
            
            SprintTween = LeanTween.value(0f, 1f,  TimeToSpint);
            SprintTween.setOnUpdate(t =>
            {
                ElfTransform.position = Vector3.Lerp(startPos, startPos + ElfTransform.forward * DistanceToSprint, t);
            });
            SprintTween.setOnComplete(_ =>
            {
                OnFinishSprint.Invoke();
                StartFlyTween();
            });
        }

        private void StartFlyTween()
        {
            Vector3 startPos = ElfTransform.position;
            
            FlyTween = LeanTween.value(0f, 1f,  TimeToFly);
            FlyTween.setOnUpdate(t =>
            {
                ElfTransform.position = Vector3.Lerp(startPos, startPos + ElfTransform.forward * DistanceToFly, t);
            });
            FlyTween.setOnComplete(_ =>
            {
                StartCurveTween();
            });
        }
        
        private void StartCurveTween()
        {
            Vector3 startPos = ElfTransform.position;
            Vector3 endPos = ElfTransform.position +
                             new Vector3(0f, DistanceToCurveVertical, DistanceToCurveHorizontal);
            Quaternion startRot = ElfTransform.rotation;
            
            //Horizontal Movement of Curve
            CurveTweenHorizontal = LeanTween.value(0f, 1f,  TimeToCurve);
            CurveTweenHorizontal.setOnUpdate(t =>
            {
                float posZ = Mathf.Lerp(startPos.z, endPos.z, t);
                ElfTransform.position = new Vector3(ElfTransform.position.x, ElfTransform.position.y, posZ);
            });
            CurveTweenHorizontal.setOnComplete(_ =>
            {
            });
            CurveTweenHorizontal.setEase(CurveHorizontalEase);
            
            //Vertical Movement of Curve
            CurveTweenVertical = LeanTween.value(0f, 1f,  TimeToCurve);
            CurveTweenVertical.setOnUpdate(t =>
            {
                float posY = Mathf.Lerp(startPos.y, endPos.y, t);
                ElfTransform.position = new Vector3(ElfTransform.position.x, posY, ElfTransform.position.z);
            });
            CurveTweenVertical.setOnComplete(_ =>
            {
            });
            CurveTweenVertical.setEase(CurveVerticalEase);
            
            //RotationX of Curve
            CurveTweenRotationX = LeanTween.value(0f, 1f,  TimeToCurve);
            CurveTweenRotationX.setOnUpdate(t =>
            {
                float rotX = Mathf.Lerp(startRot.eulerAngles.x, startRot.eulerAngles.x - 180, t);
                ElfTransform.rotation = Quaternion.Euler(rotX, startRot.eulerAngles.y, startRot.eulerAngles.z);
            });
            CurveTweenRotationX.setOnComplete(_ =>
            {
                StartReturnFlightTween();
            });
            CurveTweenRotationX.setEase(CurveRotXEase);
        }

        private void StartReturnFlightTween()
        {
            Vector3 startPos = ElfTransform.position;
            Quaternion startRot = ElfTransform.rotation;
            
            //RotationZ of Curve
            CurveTweenRotationZ = LeanTween.value(0f, 1f,  TimeToFlyRotZ);
            CurveTweenRotationZ.setOnUpdate(t =>
            {
                float rotZ = Mathf.Lerp(startRot.eulerAngles.z, startRot.eulerAngles.z + 180, t);
                ElfTransform.rotation = Quaternion.Euler(startRot.eulerAngles.x, startRot.eulerAngles.y, rotZ);
            });
            CurveTweenRotationZ.setOnComplete(_ =>
            {
            });
            CurveTweenRotationZ.setEase(FlightRotZEase);
            

            ReturnFlyTween = LeanTween.value(0f, 1f,  TimeToFlyReturn);
            ReturnFlyTween.setOnUpdate(t =>
            {
                ElfTransform.position = Vector3.Lerp(startPos, startPos + ElfTransform.forward * DistanceToFlyReturn, t);
            });
            ReturnFlyTween.setOnComplete(_ =>
            {
                OnFinishFlying.Invoke();
                StartFallingTween();
            });
        }

        private void StartFallingTween()
        {
            FallingTween = LeanTween.move(ElfTransform.gameObject, TopOfLadderPos, TimeToFall);
            FallingTween.setOnComplete(_ =>
            {
                OnFinishFalling.Invoke();
                StartCheeringTween();
            });
        }

        private void StartCheeringTween()
        {
            Quaternion startRot = ElfTransform.rotation;
            float targetYRot = startRot.eulerAngles.y + 180f;

            RotateTween = LeanTween.rotateY(ElfTransform.gameObject, targetYRot, RotateTime);
            CheeringTween = LeanTween.value(0f, 1f, CheeringTime);
            CheeringTween.setOnComplete(_ =>
            {
                OnFinishCheering.Invoke();
                StartRallyTween();
            });

        }
    }
    
    
    
    
}
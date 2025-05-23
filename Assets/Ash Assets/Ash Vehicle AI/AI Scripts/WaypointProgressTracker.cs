using System;
using UnityEngine;
using System.Linq;
namespace AshVP
{
    public class WaypointProgressTracker : MonoBehaviour
    {
        // This script can be used with any object that is supposed to follow a
        // route marked out by waypoints.
        //public float timeO;

        // This script manages the amount to look ahead along the route,
        // and keeps track of progress and laps.

        public WaypointCircuit circuit; // A reference to the waypoint-based route we should follow

        [SerializeField] private float lookAheadForTargetOffset = 5;
        // The offset ahead along the route that the we will aim for

        [SerializeField] private float lookAheadForTargetFactor = .1f;
        // A multiplier adding distance ahead along the route to aim for, based on current speed

        private float lookAheadForSpeedOffset = 50;
        // The offset ahead only the route for speed adjustments (applied as the rotation of the waypoint target transform)

        private float lookAheadForSpeedFactor = .2f;
        // A multiplier adding distance ahead along the route for speed adjustments

        [SerializeField] private ProgressStyle progressStyle = ProgressStyle.SmoothAlongRoute;
        // whether to update the position smoothly along the route (good for curved paths) or just when we reach each waypoint.

        private float pointToPointThreshold = 4;
        // proximity to waypoint which must be reached to switch target to next waypoint : only used in PointToPoint mode.

        public enum ProgressStyle
        {
            SmoothAlongRoute,
            PointToPoint,
        }

        // these are public, readable by other objects - i.e. for an AI to know where to head!
        public WaypointCircuit.RoutePoint targetPoint { get; private set; }
        public WaypointCircuit.RoutePoint speedPoint { get; private set; }
        public WaypointCircuit.RoutePoint progressPoint { get; private set; }

        public Transform target;

        [HideInInspector]
        public float progressDistance; // The progress round the route, used in smooth mode.
        private int progressNum; // the current waypoint number, used in point-to-point mode.
        private Vector3 lastPosition; // Used to calculate current speed (since we may not have a rigidbody component)
        private float speed; // current speed of this object (calculated from delta since last frame)

        // setup script properties
        private void Start()
        {

            // we use a transform to represent the point to aim for, and the point which
            // is considered for upcoming changes-of-speed. This allows this component
            // to communicate this information to the AI without requiring further dependencies.

            // You can manually create a transform and assign it to this component *and* the AI,
            // then this component will update it, and the AI can read it.
            if (target == null)
            {
                target = new GameObject(name + " Waypoint Target").transform;
            }

            Reset();
            if(circuit == null)
            {
                circuit = FindObjectOfType<WaypointCircuit>();
            }
            
        }

         
        
        
        // reset the object to sensible values
        public void Reset()
        {
            progressDistance = GetClosestWaypointToCar();
            //progressNum = circuit.ClosestWaypointNum(transform);
            progressNum = GetClosestWaypointToCar();
            if (progressStyle == ProgressStyle.PointToPoint)
            {
                target.position = circuit.Waypoints[progressNum].position;
                target.rotation = circuit.Waypoints[progressNum].rotation;
            }
        }


        private void Update()
        {
            if (transform.GetComponent<AiCarContrtoller>().grounded)
            {
                //respawnOnRoad(); //if offroad and grounded the respawn on road 
            }

            if (progressStyle == ProgressStyle.SmoothAlongRoute)
            {
                // determine the position we should currently be aiming for
                // (this is different to the current progress position, it is a a certain amount ahead along the route)
                // we use lerp as a simple way of smoothing out the speed over time.
                if (Time.deltaTime > 0)
                {
                    speed = GetComponent<AiCarContrtoller>().carVelocity.z;
                }
                target.position =
                    circuit.GetRoutePoint(progressDistance + lookAheadForTargetOffset + lookAheadForTargetFactor * speed)
                           .position;
                target.rotation =
                    Quaternion.LookRotation(
                        circuit.GetRoutePoint(progressDistance + lookAheadForSpeedOffset + lookAheadForSpeedFactor * speed)
                               .direction);


                // get our current progress along the route
                progressPoint = circuit.GetRoutePoint(progressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;
                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    progressDistance += progressDelta.magnitude * 0.5f;
                }

                lastPosition = transform.position;
            }
            else
            {
                // point to point mode. Just increase the waypoint if we're close enough:

                Vector3 targetDelta = target.position - transform.position;
                if (targetDelta.magnitude < pointToPointThreshold)
                {
                    progressNum = (progressNum + 1) % circuit.Waypoints.Length;
                }


                target.position = circuit.Waypoints[progressNum].position;
                target.rotation = circuit.Waypoints[progressNum].rotation;

                // get our current progress along the route
                progressPoint = circuit.GetRoutePoint(progressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;
                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    progressDistance += progressDelta.magnitude;
                }
                lastPosition = transform.position;
            }
            
        }

     
        private int GetClosestWaypointToCar()
        {
            Vector3 vehicleForward = transform.forward;

            
            var frontObjects = circuit.Waypoints
                .Where(obj => Vector3.Dot(vehicleForward, (obj.transform.position - transform.position).normalized) > 0) // Ön tarafta olanlar
                .OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position)) // Mesafeye göre sırala
                .ToList();
            
            
           if (frontObjects == null) return -1;
           
           var nearestObject = frontObjects.First();

           
           int index = System.Array.IndexOf(circuit.Waypoints, nearestObject);
           return index;
        }

        private void OnDrawGizmos()
        {
           /* if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(circuit.GetRoutePosition(progressDistance), 0.2f);
                Gizmos.DrawLine(transform.position, circuit.GetRoutePosition(progressDistance));
                Gizmos.DrawLine(target.position, target.position + target.forward);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(target.position, 1);
            }
*/


        }

        /* public void respawnOnRoad()
         {
             if ( Vector3.Distance( transform.position, circuit.GetRoutePosition(progressDistance)) > 15)
             {
                 timeO += Time.deltaTime;
             }
             else
             {
                 timeO = 0f;
             }
             if (timeO > 3)
             {
                 transform.position = target.position + new Vector3(0, 1.5f, 0);
                 timeO = 0f;
             }

         }*/
    }
}


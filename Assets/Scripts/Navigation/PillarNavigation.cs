using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Applied only to the four permanent pillars. Other obstacles retain their existing navigation.
[RequireComponent(typeof(BoxCollider2D))]
public sealed class PillarNavigation : MonoBehaviour
{
    private static readonly HashSet<PillarNavigation> active = new HashSet<PillarNavigation>();
    private BoxCollider2D solid;
    private void Awake() => solid = GetComponent<BoxCollider2D>();
    private void OnEnable() => active.Add(this);
    private void OnDisable() => active.Remove(this);

    public sealed class Approach
    {
        private readonly Collider2D[] neighbours = new Collider2D[16];
        private Vector3 goal;
        private Vector3 targetPosition;
        private float refreshAt;
        private bool hasGoal;
        private float originalRadius = -1;

        public bool TryGetDestination(Transform target, EnemyAttack attack, Collider2D body, NavMeshAgent agent, out Vector3 destination)
        {
            destination = target.position;
            if (attack.IsRanged) return false;
            var targetBody = target.GetComponent<Collider2D>();
            if (targetBody == null) return false;
            if (originalRadius < 0) originalRadius = agent.radius;
            bool near = false;
            bool moverNear = false;
            foreach (var pillar in active)
            {
                if (pillar.solid == null) continue;
                near |= Vector2.Distance(pillar.solid.bounds.ClosestPoint(targetBody.bounds.center), targetBody.bounds.center) < 0.6f;
                moverNear |= Vector2.Distance(pillar.solid.bounds.ClosestPoint(body.bounds.center), body.bounds.center) < 1.5f;
            }
            // Local steering must fit the baked clearance; calibrated pillar proxies already include the physical foot size.
            agent.radius = moverNear ? Mathf.Min(originalRadius, NavMesh.GetSettingsByID(agent.agentTypeID).agentRadius) : originalRadius;
            if (!near) { hasGoal = false; return false; }
            if (hasGoal && (Time.time < refreshAt || Vector2.Distance(agent.transform.position, goal) < 0.2f) && (target.position - targetPosition).sqrMagnitude < 0.0225f)
            { destination = goal; return true; }

            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            var contacts = new ContactFilter2D { useLayerMask = true, layerMask = LayerMask.GetMask("Enemy"), useTriggers = false };
            Vector3 footOffset = body.bounds.center - body.transform.position;
            var path = new NavMeshPath();
            float best = float.PositiveInfinity;
            Vector3 selected = goal;
            for (int ring = 0; ring < 2; ring++)
            for (int step = 0; step < 24; step++)
            {
                float angle = step * Mathf.PI / 12;
                float radius = attack.AttackRange * (ring == 0 ? 0.8f : 0.55f);
                Vector3 candidate = target.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                candidate.z = agent.transform.position.z;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 0.15f, filter) || Vector2.Distance(candidate, hit.position) > 0.12f || !attack.CanAttackFromPosition(target, hit.position)) continue;
                if (!NavMesh.CalculatePath(agent.transform.position, hit.position, filter, path) || path.status != NavMeshPathStatus.PathComplete) continue;
                float score = 0;
                var corners = path.corners;
                for (int i = 1; i < corners.Length; i++) score += Vector3.Distance(corners[i-1], corners[i]);
                int count = Physics2D.OverlapCircle(hit.position + footOffset, agent.radius * 1.4f, contacts, neighbours);
                for (int i = 0; i < count; i++) if (!neighbours[i].transform.IsChildOf(body.transform)) score += 4;
                if (hasGoal && Vector2.Distance(hit.position, goal) < 0.2f) score -= 0.4f;
                if (score < best) { best = score; selected = hit.position; }
            }
            if (float.IsPositiveInfinity(best)) { hasGoal = false; return false; }
            goal = selected;
            targetPosition = target.position;
            refreshAt = Time.time + 0.5f;
            hasGoal = true;
            destination = goal;
            return true;
        }
    }
}

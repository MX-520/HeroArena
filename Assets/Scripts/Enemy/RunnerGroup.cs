using System.Collections.Generic;

// One instance belongs to one enemy archetype within one wave. No scene-wide or static enemy registry.
public sealed class RunnerGroup
{
    private readonly HashSet<EnemyController> members = new HashSet<EnemyController>();
    private bool alerted;

    public void Add(EnemyController enemy)
    {
        if (enemy == null || !enemy.UsesPersistentPursuit || enemy.IsDead)
            return;
        members.Add(enemy);
        if (alerted)
            enemy.LockTarget();
    }

    public void Remove(EnemyController enemy) => members.Remove(enemy);

    public void Alert()
    {
        alerted = true;
        foreach (EnemyController enemy in members)
        {
            if (enemy != null)
                enemy.LockTarget();
        }
    }
}

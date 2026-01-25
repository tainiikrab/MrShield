using UnityEngine;

public class EnemySpawner : ObjectSpawner, IHasTarget
{
    public override void Spawn(int amount)
    {
        while (amount-- > 0)
        {
            var randomPos = GetRandomPos();

            var spawnedEnemy = Instantiate(objectPrefab, randomPos, Quaternion.identity);
            if (spawnedEnemy.TryGetComponent<IHasTarget>(out var component)) component.Target = Target;
            else throw new MissingComponentException($"{spawnedEnemy.name} must implement {nameof(IHasTarget)}");
        }
    }
}
using UnityEngine;

public interface IDamagable
{
    int Health { get; set; }

    void Damage(int amount);
}

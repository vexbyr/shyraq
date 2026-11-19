using UnityEngine;

// Общая база для всех врагов: здоровье, урон от Жарқыла и что выпадает после смерти.
// Наследник Lightable — значит, луч фонаря «видит» любого врага (IsLit), и каждый враг может замирать в свете.
// Наследники: Spirit, а дальше — Көлеңке-қасқыр, Албасты, Қара бура.
public class Enemy : Lightable
{
    [SerializeField] protected int hp = 1;          // сколько Жарқылов выдерживает
    [SerializeField] protected GameObject drop;     // что выронить после смерти (малый дух — бутылочку), пусто — ничего

    // Попал Жарқыл (вызывает Flash)
    public void TakeHit(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            if (drop != null) Instantiate(drop, transform.position, Quaternion.identity);   // добыча на месте врага
            Destroy(gameObject);
        }
    }
}

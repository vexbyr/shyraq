using UnityEngine;

// Общая база для всех врагов: здоровье, урон от Жарқыла и что выпадает после смерти.
// Наследник Lightable — значит, луч фонаря «видит» любого врага (IsLit), и каждый враг может замирать в свете.
// Наследники: Spirit, Көлеңке-қасқыр, Албасты, Қара бура.
// Числа баланса лежат в ассете EnemyStats (или его наследнике). Ссылку на ассет хранит
// сам наследник (у волка — ShadowWolfStats и т.д.) и отдаёт её через Stats().
public abstract class Enemy : Lightable
{
    [SerializeField] protected GameObject drop;     // что выронить после смерти (малый дух — бутылочку), пусто — ничего

    [Header("Звук замаха (у духа пусто: он не замахивается)")]
    [SerializeField] protected SfxPlayer sfx;       // на этом же префабе, AudioSource → группа SFX
    [SerializeField] protected AudioClip[] windUpClips;

    protected int hp;                               // текущее здоровье (только во время игры, в ассет не пишем)

    // Каждый наследник возвращает свой ассет баланса
    protected abstract EnemyStats Stats();

    protected virtual void Awake()
    {
        // Нет ассета — сразу понятная ошибка вместо NullReferenceException в Update
        if (Stats() == null)
        {
            Debug.LogError(name + ": не назначен ассет баланса (Stats)", this);
            enabled = false;
            return;
        }
        hp = Stats().maxHP;
    }

    // Свой маршрут патруля от LevelLoader: смещения от точки появления.
    // По умолчанию ничего не делает — у Албасты и Буры маршрута нет. Волк и дух переопределяют
    public virtual void SetRoute(Vector2 start, Vector2 end)
    {
    }

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

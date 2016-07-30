using UnityEngine;
using System.Collections;

public class HordeSlowdownPowerup : Powerup {
    [Range (0f, 1f)]
    public float hordeSpeedMultiplier = 0.5f;

    EnemyHorde m_horde;
    float m_startingHordeSpeed;

    protected override void OnTriggered() {
        base.OnTriggered ();

        m_horde = FindObjectOfType<EnemyHorde> ();
        m_startingHordeSpeed = m_horde.MaxHordeSpeed;
        m_horde.MaxHordeSpeed *= hordeSpeedMultiplier;
    }

    protected override void OnExpired() {
        base.OnExpired ();

        // Account for any speed changes applied after this was triggered (e.g. from jumping over enemies)
        float speedChangeAfterTriggered = m_horde.MaxHordeSpeed - (m_startingHordeSpeed * hordeSpeedMultiplier);
        m_horde.MaxHordeSpeed = m_startingHordeSpeed + speedChangeAfterTriggered;
    }
}

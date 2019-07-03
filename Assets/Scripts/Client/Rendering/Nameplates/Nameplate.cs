﻿using System;
using Common;
using Core;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

using EventHandler = Common.EventHandler;

namespace Client
{
    public class Nameplate : MonoBehaviour
    {
        [SerializeField, UsedImplicitly] private Canvas canvas;
        [SerializeField, UsedImplicitly] private CanvasGroup generalCanvasGroup;
        [SerializeField, UsedImplicitly] private AttributeBar health;
        [SerializeField, UsedImplicitly] private GameObject healthFrame;
        [SerializeField, UsedImplicitly] private GameObject contentFrame;
        [SerializeField, UsedImplicitly] private CastFrame castFrame;
        [SerializeField, UsedImplicitly] private TextMeshProUGUI unitName;
        [SerializeField, UsedImplicitly] private CameraReference cameraReference;
        [SerializeField, UsedImplicitly] private RenderingReference renderReference;
        [SerializeField, UsedImplicitly] private NameplateSettings nameplateSettings;

        private readonly Action<EntityAttributes> onAttributeChangedAction;
        private readonly Action onFactionChangedAction;

        private UnitRenderer UnitRenderer { get; set; }
        private NameplateSettings.HostilitySettings HostilitySettings { get; set; }

        private Nameplate()
        {
            onAttributeChangedAction = OnAttributeChanged;
            onFactionChangedAction = OnFactionChanged;
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            GameObjectPool.Return(this, true);
        }

        public void UpdateUnit(UnitRenderer unitRenderer)
        {
            canvas.worldCamera = cameraReference.WarcraftCamera.Camera;

            if (UnitRenderer != null)
                Deinitialize();

            if (unitRenderer != null)
                Initialize(unitRenderer);

            canvas.enabled = UnitRenderer != null;
        }

        public void UpdateSelection()
        {
            Player referer = renderReference.Player;
            Unit target = UnitRenderer.Unit;

            generalCanvasGroup.alpha = referer.Target == target
                ? HostilitySettings.SelectedGeneralAlpha
                : HostilitySettings.DeselectedGeneralAlpha;
        }

        public void DoUpdate()
        {
            transform.rotation = Quaternion.LookRotation(canvas.worldCamera.transform.forward);

            if (castFrame.gameObject.activeSelf)
                castFrame.DoUpdate();

            ApplyScaling();
        }

        private void ApplyScaling()
        {
            if (!HostilitySettings.ApplyScaling)
                return;

            WarcraftCamera warcraftCamera = cameraReference.WarcraftCamera;
            if (warcraftCamera != null)
            {
                Vector3 direction = transform.position - warcraftCamera.transform.position;
                float distance = Vector3.Dot(direction, warcraftCamera.transform.forward);

                transform.rotation = Quaternion.LookRotation(warcraftCamera.transform.forward);
                contentFrame.transform.localScale = Vector3.one * nameplateSettings.ScaleOverDistance.Evaluate(distance);
            }
        }

        private void Initialize(UnitRenderer unitRenderer)
        {
            UnitRenderer = unitRenderer;

            unitName.text = unitRenderer.Unit.Name;
            castFrame.UpdateCaster(unitRenderer.Unit);

            OnAttributeChanged(EntityAttributes.Health);
            OnFactionChanged();

            EventHandler.RegisterEvent(UnitRenderer.Unit, GameEvents.UnitAttributeChanged, onAttributeChangedAction);
            EventHandler.RegisterEvent(UnitRenderer.Unit, GameEvents.UnitFactionChanged, onFactionChangedAction);
        }

        private void Deinitialize()
        {
            EventHandler.RegisterEvent(UnitRenderer.Unit, GameEvents.UnitFactionChanged, onFactionChangedAction);
            EventHandler.UnregisterEvent(UnitRenderer.Unit, GameEvents.UnitAttributeChanged, onAttributeChangedAction);

            castFrame.UpdateCaster(null);

            UnitRenderer = null;
        }

        private void OnAttributeChanged(EntityAttributes attributeType)
        {
            if (attributeType == EntityAttributes.Health || attributeType == EntityAttributes.MaxHealth)
                health.Ratio = UnitRenderer.Unit.HealthRatio;
        }

        private void OnFactionChanged()
        {
            Player referer = renderReference.Player;
            Unit target = UnitRenderer.Unit;

            if (referer == target)
                HostilitySettings = nameplateSettings.Self;
            else if (referer.IsHostileTo(target))
                HostilitySettings = nameplateSettings.Enemy;
            else if (referer.IsFriendlyTo(target))
                HostilitySettings = nameplateSettings.Friendly;
            else
                HostilitySettings = nameplateSettings.Neutral;

            castFrame.gameObject.SetActive(HostilitySettings.ShowCast);
            unitName.gameObject.SetActive(HostilitySettings.ShowName);
            healthFrame.gameObject.SetActive(HostilitySettings.ShowHealth);
            health.FillImage.color = HostilitySettings.HealthColor;
            unitName.color = HostilitySettings.NameColor;

            UpdateSelection();

            ApplyScaling();
        }
    }
}
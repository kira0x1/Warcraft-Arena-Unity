﻿using JetBrains.Annotations;
using UnityEngine;
using Common;

namespace Client
{
    [UsedImplicitly, CreateAssetMenu(fileName = "Effect Settings", menuName = "Game Data/Visuals/Effect Settings", order = 1)]
    public class EffectSettings : ScriptableObject
    {
        [SerializeField, UsedImplicitly] private EffectEntity prototype;
        [SerializeField, UsedImplicitly] private int maxAmount;

        internal EffectManager.EffectContainer EffectContainer { get; private set; }
        internal EffectEntity Prototype => prototype;
        internal int MaxAmount => maxAmount;

        internal void Initialize(EffectManager effectManager)
        {
            EffectContainer = new EffectManager.EffectContainer(this, effectManager);
        }

        internal void Deinitialize()
        {
            EffectContainer.Dispose();
            EffectContainer = null;
        }

        internal void StopEffect(EffectEntity effectEntity, bool isDestroyed)
        {
            EffectContainer.Stop(effectEntity, isDestroyed);
        }

        public IEffectEntity PlayEffect(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return PlayEffect(position, rotation, parent, out _);
        }

        public IEffectEntity PlayEffect(Vector3 position, Quaternion rotation, Transform parent, out long playId)
        {
            Assert.IsNotNull(EffectContainer, $"Effect {name} is not initialized and won't play!");

            if (EffectContainer != null)
            {
                EffectEntity newEffect = EffectContainer.Play(position, rotation, parent);
                playId = newEffect?.PlayId ?? -1;
                return newEffect;
            }

            playId = -1;
            return null;
        }
    }
}
namespace FuzzPhyte.Dialogue.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;

    public class BindingResponseTestOrchestrator : RTDialogueOrchestrator
    {
        protected override void Awake() { }
        protected override void OnEnable() { }
        public void Configure(RTExposedBinder table, GameObject prefab)
        {
            binder = table;
            UserResponseUIPrefabItem = prefab;
        }
        public void Draw(RTResponseNode node) => DrawResponseVisual(node);
    }

    public class ExposedBinderTests
    {
        private GameObject owner;
        private GameObject target;
        private RTExposedBinder binder;

        [SetUp]
        public void Setup()
        {
            owner = new GameObject("Binder test");
            target = new GameObject("Target");
            binder = owner.AddComponent<RTExposedBinder>();
        }

        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(target);
        }

        private void Store(string id)
        {
            var serialized = new SerializedObject(binder);
            var bindings = serialized.FindProperty("bindings");
            bindings.arraySize = 1;
            bindings.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = id;
            bindings.GetArrayElementAtIndex(0).FindPropertyRelative("obj").objectReferenceValue = target;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PropertyName UnknownProperty()
        {
            // An identity received from another Editor/Player without its reverse-name table.
            return new PropertyName(1758391239);
        }

        [Test]
        public void NamedSerializedBindingResolvesWithoutReverseNameTable()
        {
            var property = UnknownProperty();
            Store("EditorOnlyAnchor:" + property.GetHashCode());
            Assert.That(binder.GetReferenceValue(property, out var valid), Is.SameAs(target));
            Assert.That(valid, Is.True);
        }

        [TestCase("Car_Selection_One")]
        [TestCase("Car_Selection_Three")]
        public void ExistingNamedBindingsResolveFromString(string key)
        {
            Store(new PropertyName(key).ToString());
            Assert.That(binder.TryGet<GameObject>(key, out var found), Is.True);
            Assert.That(found, Is.SameAs(target));
        }

        [Test]
        public void UnknownSerializedBindingResolvesFromNamedProperty()
        {
            var property = new PropertyName("Car_Selection_One");
            Store("Unknown:" + property.GetHashCode());
            Assert.That(binder.GetReferenceValue(property, out var valid), Is.SameAs(target));
            Assert.That(valid, Is.True);
        }

        [Test]
        public void PlainStringBindingResolves()
        {
            Store("PlainAnchor");
            Assert.That(binder.TryGet<GameObject>("PlainAnchor", out var found), Is.True);
            Assert.That(found, Is.SameAs(target));
        }

        [Test]
        public void SetUpdatesExistingIdentityWithoutAddingOrRenamingBinding()
        {
            var property = UnknownProperty();
            var stored = "EditorOnlyAnchor:" + property.GetHashCode();
            Store(stored);
            binder.SetReferenceValue(property, owner);
            var bindings = new SerializedObject(binder).FindProperty("bindings");
            Assert.That(bindings.arraySize, Is.EqualTo(1));
            Assert.That(bindings.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue, Is.EqualTo(stored));
            Assert.That(bindings.GetArrayElementAtIndex(0).FindPropertyRelative("obj").objectReferenceValue, Is.SameAs(owner));
        }

        [Test]
        public void ClearRemovesSerializedIdentityWhenDisplayNameIsUnavailable()
        {
            var property = UnknownProperty();
            Store("EditorOnlyAnchor:" + property.GetHashCode());
            binder.ClearReferenceValue(property);
            Assert.That(new SerializedObject(binder).FindProperty("bindings").arraySize, Is.Zero);
            Assert.That(binder.GetReferenceValue(property, out var valid), Is.Null);
            Assert.That(valid, Is.False);
        }

        [Test]
        public void RepeatedMissingLookupsStayInvalid()
        {
            for (int i = 0; i < 2; i++)
            {
                Assert.That(binder.GetReferenceValue(new PropertyName("MissingAnchor"), out var valid), Is.Null);
                Assert.That(valid, Is.False);
            }
        }

        [Test]
        public void DifferentIdentitiesDoNotMatch()
        {
            Store(new PropertyName("ExpectedAnchor").ToString());
            Assert.That(binder.TryGet<GameObject>("DifferentAnchor", out var found), Is.False);
            Assert.That(found, Is.Null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingResponseAnchorOrPrefabLogsAnError(bool missingPrefab)
        {
            if (missingPrefab) binder.SetReferenceValue(new PropertyName("ResponseAnchor"), target);
            var orchestrator = owner.AddComponent<BindingResponseTestOrchestrator>();
            orchestrator.Configure(binder, missingPrefab ? null : target);
            var prompts = new List<RTSinglePromptNode>
            {
                new RTSinglePromptNode("prompt", default, null, null, null, "ResponseAnchor")
            };
            var node = new RTResponseNode("response", true, default, prompts,
                new List<RTFPNodePort> { default }, null, null);
            LogAssert.Expect(LogType.Error, new Regex("Cannot spawn dialogue response 0 at node response: lookup 'ResponseAnchor'"));
            orchestrator.Draw(node);
            Assert.That(target.transform.childCount, Is.Zero);
        }
    }
}

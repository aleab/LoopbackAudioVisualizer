using Aleab.LoopbackAudioVisualizer.Helpers;
using Aleab.LoopbackAudioVisualizer.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aleab.LoopbackAudioVisualizer.Scripts.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class TabMenu : UIBehaviour
    {
        #region Inspector

#pragma warning disable 0649

        [SerializeField]
        [DisableWhenPlaying]
        private RectTransform tabsRect;

        [SerializeField]
        [DisableWhenPlaying]
        private Image contentArea;

        [SerializeField]
        [DisableWhenPlaying]
        private TabMenuTab tabPrefab;

        [Header("Tab Sprites")]
        [SerializeField]
        [DisableWhenPlaying]
        private Sprite firstTabSprite;

        [SerializeField]
        [DisableWhenPlaying]
        private Sprite middleTabSprite;

        [SerializeField]
        [DisableWhenPlaying]
        private Sprite lastTabSprite;

        [SerializeField]
        [DisableWhenPlaying]
        private Sprite singleTabSprite;

        [Header("Colors")]
        [SerializeField]
        [DisableWhenPlaying]
        private Color activeColor = Color.white;

        [SerializeField]
        [DisableWhenPlaying]
        private Color inactiveColor = Color.gray;

#pragma warning restore 0649

        #endregion Inspector

        private readonly HashSet<int> runtimeAddedItems = new HashSet<int>();

        private TabMenuItem[] tabMenuItems;
        private TabMenuTab[] tabMenuTabs;

        private int currentTabIndex;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            this.RequireField(nameof(this.tabsRect), this.tabsRect);
            this.RequireField(nameof(this.contentArea), this.contentArea);
            this.RequireField(nameof(this.tabPrefab), this.tabPrefab);

            this.RequireField(nameof(this.firstTabSprite), this.firstTabSprite);
            this.RequireField(nameof(this.middleTabSprite), this.middleTabSprite);
            this.RequireField(nameof(this.lastTabSprite), this.lastTabSprite);
            this.RequireField(nameof(this.singleTabSprite), this.singleTabSprite);
        }

        /// <inheritdoc />
        protected override void Start()
        {
            base.Start();

            this.contentArea.color = this.activeColor;
            this.CreateTabs();
        }

        public void RemoveNonDefaultItems()
        {
            if (this.tabMenuItems == null)
                return;
            if (this.runtimeAddedItems.Count == 0)
                return;

            var items = this.tabMenuItems.Where(item => this.runtimeAddedItems.Contains(item.GetInstanceID())).ToArray();
            if (items.Length > 0)
            {
                foreach (var item in items)
                    Destroy(item.gameObject);

                this.RemoveTabs();
                this.CreateTabs();
            }
        }

        public void AddItem(TabMenuItem item)
        {
            if (this.AddItemInternal(item))
                this.CreateTabs();
        }

        public void AddItems(IEnumerable<TabMenuItem> items)
        {
            if (items.Aggregate(false, (shouldRecreateTabs, item) => shouldRecreateTabs | this.AddItemInternal(item)))
                this.CreateTabs();
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private bool AddItemInternal(TabMenuItem item)
        {
            this.runtimeAddedItems.Add(item.GetInstanceID());
            item.gameObject.transform.parent = this.contentArea.gameObject.transform;
            if (this.tabMenuTabs != null)
            {
                this.RemoveTabs();
                return true;
            }
            return false;
        }

        private void CreateTabs()
        {
            this.tabMenuItems = this.contentArea.GetComponentsInChildren<TabMenuItem>(true);
            this.tabMenuTabs = new TabMenuTab[this.tabMenuItems.Length];
            for (int i = 0; i < this.tabMenuItems.Length; ++i)
            {
                TabMenuTab tab = Instantiate(this.tabPrefab, this.tabsRect);
                this.tabMenuTabs[i] = tab;

                // Set text
                tab.SetText(this.tabMenuItems[i].TabName);

                // Set sprite
                if (i == 0 && i == this.tabMenuItems.Length - 1)
                    tab.Sprite = this.singleTabSprite;
                else if (i == 0)
                    tab.Sprite = this.firstTabSprite;
                else if (i == this.tabMenuItems.Length - 1)
                    tab.Sprite = this.lastTabSprite;
                else
                    tab.Sprite = this.middleTabSprite;

                // Set color and position
                this.UpdateTabPositionAndColor(tab, 0.0f, index: i);

                tab.Resized += this.TabMenuTab_Resized;
                tab.Clicked += this.TabMenuTab_Clicked;
            }
        }

        private void RemoveTabs()
        {
            if (this.tabMenuTabs == null)
                return;

            foreach (var tab in this.tabMenuTabs)
                Destroy(tab.gameObject);
            Array.Clear(this.tabMenuTabs, 0, this.tabMenuTabs.Length);
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private void UpdateCurrentTabIndex(TabMenuTab currentTab)
        {
            this.currentTabIndex = this.tabMenuTabs.ToList().FindIndex(t => t.GetInstanceID() == currentTab.GetInstanceID());
        }

        private void UpdateTabPositionAndColor(TabMenuTab tab, float x, int? index = null, bool? selected = null)
        {
            bool isCurrent = selected ?? (index != null
                ? index == this.currentTabIndex
                : this.tabMenuTabs.ToList().FindIndex(t => t.GetInstanceID() == tab.GetInstanceID()) == this.currentTabIndex);

            tab.Color = isCurrent ? this.activeColor : this.inactiveColor;
            tab.HighlightedColor = isCurrent ? Color.white : Color.HSVToRGB(0.0f, 0.0f, 0.9375f);
            this.MoveTab(tab, x, isCurrent);
        }

        private void MoveTab(TabMenuTab tab, float x)
        {
            bool selected = this.tabMenuTabs.ToList().FindIndex(t => t.GetInstanceID() == tab.GetInstanceID()) == this.currentTabIndex;
            this.MoveTab(tab, x, selected);
        }

        private void MoveTab(TabMenuTab tab, float x, bool selected)
        {
            tab.RectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, x, tab.RectTransform.rect.width);
            tab.RectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, selected ? -2.0f : 0.0f, this.tabsRect.rect.height);
        }

        private void TabMenuTab_Resized(object sender, EventArgs e)
        {
            if (this.tabMenuTabs == null)
                return;

            float currentX = 0.0f;
            foreach (var tab in this.tabMenuTabs)
            {
                this.MoveTab(tab, currentX);
                currentX += tab.RectTransform.rect.width;
            }
        }

        private void TabMenuTab_Clicked(object sender, PointerEventData e)
        {
            TabMenuTab selectedTab = sender as TabMenuTab;
            if (selectedTab == null)
                return;
            if (e.button != PointerEventData.InputButton.Left)
                return;

            int previousIndex = this.currentTabIndex;
            this.UpdateCurrentTabIndex(selectedTab);

            TabMenuTab previousTab = this.tabMenuTabs[previousIndex];

            this.UpdateTabPositionAndColor(previousTab, previousTab.RectTransform.offsetMin.x, selected: false);
            this.UpdateTabPositionAndColor(selectedTab, selectedTab.RectTransform.offsetMin.x, selected: true);

            this.tabMenuItems[previousIndex].gameObject.SetActive(false);
            this.tabMenuItems[this.currentTabIndex].gameObject.SetActive(true);
        }
    }
}
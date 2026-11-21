using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace GoodNotesClone.Models
{
    public class Notebook : ICloneable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Untitled Notebook";
        public string FilePath { get; set; } = string.Empty;
        public Color CoverColor { get; set; } = Colors.Blue;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public PageCollection Pages { get; set; } = new PageCollection();
        public int CurrentPageIndex { get; set; } = 0;
        public double Width { get; set; } = 800;
        public double Height { get; set; } = 1131;
        public bool IsReadOnly { get; set; } = false;
        
        public Notebook() { }
        
        public Notebook(string name)
        {
            Name = name;
        }
        
        public Page CurrentPage => Pages.Count > 0 ? Pages[CurrentPageIndex] : null;
        
        public void AddPage(Page page)
        {
            Pages.Add(page);
            page.Index = Pages.Count - 1;
            ModifiedDate = DateTime.Now;
        }
        
        public void AddPage(PageTemplate template)
        {
            var page = new Page($"Page {Pages.Count + 1}", template, Width, Height);
            AddPage(page);
        }
        
        public void RemovePage(Page page)
        {
            Pages.Remove(page);
            UpdatePageIndices();
            ModifiedDate = DateTime.Now;
            if (CurrentPageIndex >= Pages.Count)
                CurrentPageIndex = Math.Max(0, Pages.Count - 1);
        }
        
        public void RemovePageAt(int index)
        {
            if (index >= 0 && index < Pages.Count)
            {
                Pages.RemoveAt(index);
                UpdatePageIndices();
                ModifiedDate = DateTime.Now;
                if (CurrentPageIndex >= Pages.Count)
                    CurrentPageIndex = Math.Max(0, Pages.Count - 1);
            }
        }
        
        private void UpdatePageIndices()
        {
            for (int i = 0; i < Pages.Count; i++)
                Pages[i].Index = i;
        }
        
        public void GoToPage(int index)
        {
            if (index >= 0 && index < Pages.Count)
                CurrentPageIndex = index;
        }
        
        public void GoToNextPage() => GoToPage(CurrentPageIndex + 1);
        public void GoToPreviousPage() => GoToPage(CurrentPageIndex - 1);
        
        public void Clear()
        {
            Pages.Clear();
            AddPage(PageTemplate.Blank);
            CurrentPageIndex = 0;
            ModifiedDate = DateTime.Now;
        }
        
        public Notebook Clone()
        {
            var clone = new Notebook
            {
                Id = Guid.NewGuid(),
                Name = Name + " (Copy)",
                FilePath = string.Empty,
                CoverColor = CoverColor,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Width = Width,
                Height = Height,
                CurrentPageIndex = 0
            };
            foreach (var page in Pages)
                clone.Pages.Add(page.Clone());
            return clone;
        }
        
        object ICloneable.Clone() => Clone();
    }
    
    public class PageCollection : List<Page>
    {
        public PageCollection() { }
        public PageCollection(IEnumerable<Page> pages) : base(pages) { }
        
        public Page FindById(Guid id) => Find(p => p.Id == id);
        public int GetIndexById(Guid id) => FindIndex(p => p.Id == id);
        public void MovePage(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= Count || newIndex < 0 || newIndex >= Count)
                return;
            var page = this[oldIndex];
            RemoveAt(oldIndex);
            Insert(newIndex, page);
            for (int i = 0; i < Count; i++)
                this[i].Index = i;
        }
    }
}

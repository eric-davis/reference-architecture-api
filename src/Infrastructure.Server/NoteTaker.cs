namespace CompanyName.Notebook.NoteTaking.Infrastructure.Server
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using CompanyName.Notebook.NoteTaking.Core.Application.Exceptions;
    using CompanyName.Notebook.NoteTaking.Core.Application.Messages;
    using CompanyName.Notebook.NoteTaking.Core.Application.Services;
    using CompanyName.Notebook.NoteTaking.Core.Domain.Factories;
    using CompanyName.Notebook.NoteTaking.Core.Domain.Models;
    using CompanyName.Notebook.NoteTaking.Core.Domain.Services;

    public class NoteTaker : INoteTaker
    {
        private readonly INoteFactory _noteFactory;
        private readonly ISubscriberFactory _subscriberFactory;
        private readonly ICategoryFactory _categoryFactory;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public NoteTaker(
            INoteFactory noteFactory,
            ISubscriberFactory subscriberFactory,
            ICategoryFactory categoryFactory,
            ICategoryRepository categoryRepository,
            IMapper mapper
        )
        {
            _noteFactory = noteFactory ?? throw new ArgumentNullException(nameof(noteFactory));
            _subscriberFactory = subscriberFactory ?? throw new ArgumentNullException(nameof(subscriberFactory));
            _categoryFactory = categoryFactory ?? throw new ArgumentNullException(nameof(categoryFactory));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        //
        // Work with notes with regard for categories.
        // In esseence these notes will be stored in the context of default category.

        public NoteDto TakeNote(NewNoteMessage newNoteMessage)
        {
            var category = GetDefaultCategory();

            var note = category.AddNote(newNoteMessage.Text);
            _categoryRepository.Save(category);

            return _mapper.Map<NoteDto>(note);
        }

        public NoteDto ReadNote(Guid noteId)
        {
            var category = GetDefaultCategory();

            var note = category.RevealNote(noteId);

            return _mapper.Map<NoteDto>(note);
        }

        public void RemoveNote(Guid noteId)
        {
            var category = GetDefaultCategory();

            category.RemoveNote(noteId);
            _categoryRepository.Save(category);
        }

        public IList<NoteDto> ListNotes()
        {
            var category = GetDefaultCategory();

            var notes = category.Notes;

            return _mapper.Map<IList<NoteDto>>(notes);
        }

        //
        // Work with categories

        public IList<CategoryDto> ListCategories()
        {
            var categories = _categoryRepository.GetAll();
            if (null == categories) throw new NotFoundException("No Categories found.");

            return _mapper.Map<IList<CategoryDto>>(categories);
        }

        public CategoryDto CreateNewCategory(NewCategoryMessage newCategoryMessage)
        {
            if (newCategoryMessage == null)
            {
                throw new ArgumentNullException(nameof(newCategoryMessage));
            }

            var category = _categoryFactory.Create(
                newCategoryMessage.Name,
                _noteFactory,
                _subscriberFactory
            );

            category = _categoryRepository.Add(category);
            
            return _mapper.Map<CategoryDto>(category);
        }

        public CategoryDto RenameCategory(Guid categoryId, string newCategoryName)
        {
            if (string.IsNullOrEmpty(newCategoryName))
            {
                throw new ArgumentException("message", nameof(newCategoryName));
            }

            var category = _categoryRepository.Get(categoryId);
            if (null == category) throw new NotFoundException("Category not found.");

            category = _categoryFactory.Build(
                category,
                _noteFactory,
                _subscriberFactory
            );

            category.ChangeName(newCategoryName);
            category = _categoryRepository.Save(category);

            return _mapper.Map<CategoryDto>(category);
        }

        public CategoryDto GetCategoryDetail(Guid categoryId)
        {
            var category = _categoryRepository.Get(categoryId);
            if (null == category) throw new NotFoundException("Category not found.");

            return _mapper.Map<CategoryDto>(category);
        }

        public void RemoveCategory(Guid categoryId)
        {
            throw new NotImplementedException();
        }

        // Work with categorized notes
        
        public CategoryDto RemoveCategorizedNote(Guid categoryId, Guid noteId)
        {
            var category = _categoryRepository.Get(categoryId);
            if (null == category) throw new NotFoundException("Category not found.");

            category = _categoryFactory.Build(
                category,
                _noteFactory,
                _subscriberFactory
            );

            category.RemoveNote(noteId);
            category = _categoryRepository.Save(category);

            return _mapper.Map<CategoryDto>(category);
        }

        public NoteDto TakeCategorizedNote(Guid categoryId, NewNoteMessage newNoteMessage)
        {
            if (newNoteMessage == null)
            {
                throw new ArgumentNullException(nameof(newNoteMessage));
            }

            var category = GetCategory(categoryId);

            var note = category.AddNote(newNoteMessage.Text);
            _categoryRepository.Save(category);

            return _mapper.Map<NoteDto>(note);
        }

        public IList<NoteDto> ListCategorizedNotes(Guid categoryId)
        {
            var category = GetCategory(categoryId);
            return _mapper.Map<IList<NoteDto>>(category.Notes);
        }

        public NoteDto ReadCategorizedNote(Guid categoryId, Guid noteId)
        {
            var category = GetCategory(categoryId);
            var note = category.RevealNote(noteId);
            return _mapper.Map<NoteDto>(note);
        }

        private ICategory GetDefaultCategory()
        {
            var defaultCategoryName = "default";
            var category = _categoryRepository.GetByName(defaultCategoryName);
            if (null == category)
            {
                category = _categoryFactory.Create(
                    "default",
                    _noteFactory,
                    _subscriberFactory
                );
                category = _categoryRepository.Add(category);
            }

            category = _categoryFactory.Build(
                category,
                _noteFactory,
                _subscriberFactory
            );

            return category;
        }

        private ICategory GetCategory(Guid categoryId)
        {
            var category = _categoryRepository.Get(categoryId);
            if (null == category) throw new NotFoundException("Category not found.");

            category = _categoryFactory.Build(
                category,
                _noteFactory,
                _subscriberFactory
            );

            return category;
        }
    }
}
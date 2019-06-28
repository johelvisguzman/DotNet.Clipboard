namespace DotNet.Clipboard.Test
{
    using DotNetToolkit.Repository;
    using DotNetToolkit.Repository.InMemory;
    using Infrastructure;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Repositories;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using ViewModels;

    [TestClass]
    public class MainWindowViewModelTests
    {
        [TestMethod]
        public void Add_Text_With_Max_Copy_Count()
        {
            var maxCopyCount = 3;
            var mockAppSettingsService = new Mock<IAppSettingsService>();
            var mockDialogService = new Mock<IDialogService>();
            var repoFactory = new RepositoryFactory(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var repo = (IClipboardRepository)repoFactory.CreateInstance<ClipboardRepository>();

            mockAppSettingsService.SetupSet(s => s.MaxSavedCopiesCount = It.IsAny<int>()).Callback<int>(s => maxCopyCount = s);
            mockAppSettingsService.SetupGet(s => s.MaxSavedCopiesCount).Returns(maxCopyCount);

            var settings = new SettingsWindowViewModel(repo, mockAppSettingsService.Object);
            var viewModel = new MainWindowViewModel(repo, mockAppSettingsService.Object, mockDialogService.Object, settings);

            viewModel.Initialize(); // this gets called when the screen is initialized for the first time (needs to be called manually for testing)

            Assert.AreEqual(0, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.AddCommand.Execute(new ClipViewModel("New Text #1", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("New Text #2", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("New Text #3", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("New Text #4", DataFormats.Text));

            Assert.AreEqual(3, viewModel.Clips.Cast<ClipViewModel>().Count());
            Assert.AreEqual("New Text #3", (string)viewModel.Clips.Cast<ClipViewModel>().ElementAt(0).Data);
            Assert.AreEqual("New Text #2", (string)viewModel.Clips.Cast<ClipViewModel>().ElementAt(1).Data);
            Assert.AreEqual("New Text #1", (string)viewModel.Clips.Cast<ClipViewModel>().ElementAt(2).Data);
        }

        [TestMethod]
        public void Add_Text_Duplicate()
        {
            var mockAppSettingsService = new Mock<IAppSettingsService>();
            var mockDialogService = new Mock<IDialogService>();
            var repoFactory = new RepositoryFactory(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var repo = (IClipboardRepository)repoFactory.CreateInstance<ClipboardRepository>();

            var settings = new SettingsWindowViewModel(repo, mockAppSettingsService.Object);
            var viewModel = new MainWindowViewModel(repo, mockAppSettingsService.Object, mockDialogService.Object, settings);

            viewModel.Initialize(); // this gets called when the screen is initialized for the first time (needs to be called manually for testing)

            Assert.AreEqual(0, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.AddCommand.Execute(new ClipViewModel("New Text #1", DataFormats.Text));

            Assert.AreEqual(1, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.AddCommand.Execute(new ClipViewModel("New Text #1", DataFormats.Text));

            Assert.AreEqual(1, viewModel.Clips.Cast<ClipViewModel>().Count());
        }

        [TestMethod]
        public void Add_Image_With_Max_Copy_Count()
        {
            var maxCopyCount = 2;
            var mockAppSettingsService = new Mock<IAppSettingsService>();
            var mockDialogService = new Mock<IDialogService>();
            var repoFactory = new RepositoryFactory(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var repo = (IClipboardRepository)repoFactory.CreateInstance<ClipboardRepository>();

            mockAppSettingsService.SetupSet(s => s.MaxSavedCopiesCount = It.IsAny<int>()).Callback<int>(s => maxCopyCount = s);
            mockAppSettingsService.SetupGet(s => s.MaxSavedCopiesCount).Returns(maxCopyCount);

            var settings = new SettingsWindowViewModel(repo, mockAppSettingsService.Object);
            var viewModel = new MainWindowViewModel(repo, mockAppSettingsService.Object, mockDialogService.Object, settings);

            viewModel.Initialize(); // this gets called when the screen is initialized for the first time (needs to be called manually for testing)

            Assert.AreEqual(0, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.AddCommand.Execute(new ClipViewModel(Utils.BitmapToBitmapSource(Properties.Resources.image_1), DataFormats.Bitmap));
            viewModel.AddCommand.Execute(new ClipViewModel(Utils.BitmapToBitmapSource(Properties.Resources.image_2), DataFormats.Bitmap));
            viewModel.AddCommand.Execute(new ClipViewModel(Utils.BitmapToBitmapSource(Properties.Resources.image_3), DataFormats.Bitmap));

            Assert.AreEqual(2, viewModel.Clips.Cast<ClipViewModel>().Count());
        }

        [TestMethod]
        public void Add_Image_Duplicate()
        {
            var mockAppSettingsService = new Mock<IAppSettingsService>();
            var mockDialogService = new Mock<IDialogService>();
            var repoFactory = new RepositoryFactory(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var repo = (IClipboardRepository)repoFactory.CreateInstance<ClipboardRepository>();

            var settings = new SettingsWindowViewModel(repo, mockAppSettingsService.Object);
            var viewModel = new MainWindowViewModel(repo, mockAppSettingsService.Object, mockDialogService.Object, settings);

            viewModel.Initialize(); // this gets called when the screen is initialized for the first time (needs to be called manually for testing)

            Assert.AreEqual(0, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.AddCommand.Execute(new ClipViewModel(Utils.BitmapToBitmapSource(Properties.Resources.image_1), DataFormats.Bitmap));
            viewModel.AddCommand.Execute(new ClipViewModel(Utils.BitmapToBitmapSource(Properties.Resources.image_1), DataFormats.Bitmap));

            Assert.AreEqual(1, viewModel.Clips.Cast<ClipViewModel>().Count());
        }

        [TestMethod]
        public void Search_Text()
        {
            var mockAppSettingsService = new Mock<IAppSettingsService>();
            var mockDialogService = new Mock<IDialogService>();
            var repoFactory = new RepositoryFactory(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var repo = (IClipboardRepository)repoFactory.CreateInstance<ClipboardRepository>();

            var settings = new SettingsWindowViewModel(repo, mockAppSettingsService.Object);
            var viewModel = new MainWindowViewModel(repo, mockAppSettingsService.Object, mockDialogService.Object, settings);

            viewModel.Initialize(); // this gets called when the screen is initialized for the first time (needs to be called manually for testing)

            viewModel.AddCommand.Execute(new ClipViewModel("Clip 1", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("Clip 10", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("Clip 2", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("Clip 22", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("Clip 222", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("Clip 3", DataFormats.Text));

            Assert.AreEqual(6, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.Search = "2";

            Assert.AreEqual(3, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.Search = "Clip 222";

            Assert.AreEqual(1, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.Search = "Clip 9";

            Assert.AreEqual(0, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.Search = "";

            Assert.AreEqual(6, viewModel.Clips.Cast<ClipViewModel>().Count());
        }

        [TestMethod]
        public void Search_Image()
        {
            var mockAppSettingsService = new Mock<IAppSettingsService>();
            var mockDialogService = new Mock<IDialogService>();
            var repoFactory = new RepositoryFactory(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var repo = (IClipboardRepository)repoFactory.CreateInstance<ClipboardRepository>();

            var settings = new SettingsWindowViewModel(repo, mockAppSettingsService.Object);
            var viewModel = new MainWindowViewModel(repo, mockAppSettingsService.Object, mockDialogService.Object, settings);

            viewModel.Initialize(); // this gets called when the screen is initialized for the first time (needs to be called manually for testing)

            viewModel.AddCommand.Execute(new ClipViewModel(Utils.BitmapToBitmapSource(Properties.Resources.image_1), DataFormats.Bitmap));
            viewModel.AddCommand.Execute(new ClipViewModel(Utils.BitmapToBitmapSource(Properties.Resources.image_2), DataFormats.Bitmap));

            Assert.AreEqual(2, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.Search = "Bitmap";

            Assert.AreEqual(2, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.Search = "Image??";

            Assert.AreEqual(0, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.Search = "";

            Assert.AreEqual(2, viewModel.Clips.Cast<ClipViewModel>().Count());
        }

        [TestMethod]
        public void Select()
        {
            var yesterday = DateTime.Today.AddDays(-1);
            var eventIsRaised = false;
            var mockAppSettingsService = new Mock<IAppSettingsService>();
            var mockDialogService = new Mock<IDialogService>();
            var repoFactory = new RepositoryFactory(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var repo = (IClipboardRepository)repoFactory.CreateInstance<ClipboardRepository>();

            var settings = new SettingsWindowViewModel(repo, mockAppSettingsService.Object);
            var viewModel = new MainWindowViewModel(repo, mockAppSettingsService.Object, mockDialogService.Object, settings);

            viewModel.Initialize(); // this gets called when the screen is initialized for the first time (needs to be called manually for testing)

            viewModel.AddCommand.Execute(new ClipViewModel("Clip 3", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("Clip 2", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("Clip 1", DataFormats.Text));

            var clipAtIndex2 = viewModel.Clips.Cast<ClipViewModel>().ElementAt(2);

            Assert.IsNotNull(clipAtIndex2);

            var addedDate = clipAtIndex2.AddedDate;
            var lastUsedDate = clipAtIndex2.LastUsedDate;

            viewModel.ClipSelected += delegate { eventIsRaised = true; };

            viewModel.SelectCommand.Execute(clipAtIndex2);

            Assert.AreEqual(clipAtIndex2, viewModel.Clips.Cast<ClipViewModel>().ElementAt(0));
            Assert.AreEqual(addedDate, clipAtIndex2.AddedDate);
            Assert.AreNotEqual(lastUsedDate, clipAtIndex2.LastUsedDate);
            Assert.IsTrue(eventIsRaised);
        }

        [TestMethod]
        public void Delete()
        {
            var mockAppSettingsService = new Mock<IAppSettingsService>();
            var mockDialogService = new Mock<IDialogService>();
            var repoFactory = new RepositoryFactory(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var repo = (IClipboardRepository)repoFactory.CreateInstance<ClipboardRepository>();

            var settings = new SettingsWindowViewModel(repo, mockAppSettingsService.Object);
            var viewModel = new MainWindowViewModel(repo, mockAppSettingsService.Object, mockDialogService.Object, settings);

            viewModel.Initialize(); // this gets called when the screen is initialized for the first time (needs to be called manually for testing)

            viewModel.AddCommand.Execute(new ClipViewModel("Clip 1", DataFormats.Text));
            viewModel.AddCommand.Execute(new ClipViewModel("Clip 2", DataFormats.Text));

            Assert.AreEqual(2, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.DeleteCommand.Execute(new List<ClipViewModel>() { viewModel.Clips.Cast<ClipViewModel>().ElementAt(0) });

            Assert.AreEqual(1, viewModel.Clips.Cast<ClipViewModel>().Count());

            viewModel.DeleteCommand.Execute(new List<ClipViewModel>() { viewModel.Clips.Cast<ClipViewModel>().ElementAt(0) });

            Assert.AreEqual(0, viewModel.Clips.Cast<ClipViewModel>().Count());

        }
    }
}

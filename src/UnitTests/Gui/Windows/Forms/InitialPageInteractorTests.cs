/* 
 * Copyright (C) 1999-2009 John K�ll�n.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */

using Decompiler.Gui;
using Decompiler.UnitTests.Mocks;
using Decompiler.Gui.Windows.Forms;
using NUnit.Framework;
using System;

namespace Decompiler.UnitTests.Gui.Windows.Forms
{
	[TestFixture]
	public class InitialPageInteractorTests
	{
		private IMainForm form;
		private IStartPage page;
		private InitialPageInteractor i;
        private FakeUiService uiSvc;

		[SetUp]
		public void Setup()
		{
			form = new MainForm();
            page = form.StartPage;
			i = new InitialPageInteractor(page);
            FakeComponentSite site = new FakeComponentSite(i);
            uiSvc = new FakeUiService();
            site.AddService(typeof(IDecompilerUIService), uiSvc);
            site.AddService(typeof(IDecompilerService), new DecompilerService());
            i.Site = site;
		}

		[TearDown]
		public void Teardown()
		{
			form.Dispose();
		}

		[Test]
		public void ClickOnBrowseBinaryFile()
		{
			page.InputFile.Text = "foo.bar";
			uiSvc.OpenFileResult = "baz\\foo.bar";
			i.BrowseInputFile_Click(null, EventArgs.Empty);

			Assert.AreEqual("baz\\foo.bar", page.InputFile.Text);
		}

		[Test]
		public void CancelBrowseBinaryFile()
		{
			page.InputFile.Text = "foo.bar";
			uiSvc.OpenFileResult = "NIX";
			uiSvc.SimulateUserCancel = true;
			i.BrowseInputFile_Click(null, EventArgs.Empty);

			Assert.AreEqual("foo.bar", page.InputFile.Text);
		}

        [Test]
        public void CanAdvance()
        {
            Assert.IsFalse(i.CanAdvance);
        }

        //$REFACTOR: copied from LoadedPageInteractor, should
        // push to base class or utility class.
        private MenuStatus QueryStatus(int cmdId)
        {
            CommandStatus status = new CommandStatus();
            i.QueryStatus(ref CmdSets.GuidDecompiler, cmdId, status, null);
            return status.Status;
        }

	}
}
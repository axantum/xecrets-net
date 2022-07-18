using System;
using System.ComponentModel;
using System.Drawing;

namespace AxCrypt.Desktop.Window
{
    partial class AxCryptMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AxCryptMainForm));
            this._progressTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._recentFilesContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._recentFilesOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._removeRecentFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._decryptAndRemoveFromListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._shareKeysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._recentFilesShowInFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._recentFilesRestoreAnonymousNamesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._clearRecentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._progressContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._progressContextCancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._watchedFoldersKeySharingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersDecryptMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersdecryptTemporarilyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersRemoveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersOpenExplorerHereMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersAddSecureFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersTabPage = new System.Windows.Forms.TabPage();
            this._watchedFoldersListView = new System.Windows.Forms.ListView();
            this._watchedFolderColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._recentFilesTabPage = new System.Windows.Forms.TabPage();
            this._recentFilesListView = new AxCrypt.Desktop.Window.RecentFilesListView();
            this._decryptedFileColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._fileSizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._lastAccessedDateColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._lastModifiedDateColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._encryptedPathColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._cryptoNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._statusTabControl = new System.Windows.Forms.TabControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openEncryptedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._encryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._decryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._encryptedFoldersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._restoreAnonymousNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addSecureFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._secureDeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._inviteUserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._englishLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._germanLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._dutchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._spanishLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._francaisLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._italianLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._koreanLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._polishLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._portugueseBrazilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._russianLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._swedishLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._turkishLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._chineseLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsChangePassphraseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._passwordResetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsEncryptionUpgradeModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._alwaysOfflineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsIncludeSubfoldersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._inactivitySignOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._disableInactivitySignOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._fiveMinuteInactivitySignOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._fifteenMinuteInactivitySignOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._thirtyMinuteInactivitySignOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._sixtyMinuteInactivitySignOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsDebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsHideRecentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsClearAllSettingsAndRestartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._keyManagementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._importOthersSharingKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exportSharingKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._importMyPrivateKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exportMyPrivateKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._createAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._encryptionUpgradeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._cleanDecryptedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._signInToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._signOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._debugCheckVersionNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._debugOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._debugCryptoPolicyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._debugLoggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._debugManageAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._tryBrokenFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._VerifyFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._axcryptFileFormatCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._debugOpenReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpViewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpAboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._checkForUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._getPremiumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._manageAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this._mainToolStripTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this._openEncryptedToolStripButton = new ImageButtonWithLabel();
            this._stopSecuringToolStripButton = new ImageButtonWithLabel();
            this._encryptToolStripButton = new ImageButtonWithLabel();
            this._keyShareToolStripButton = new ImageButtonWithLabel();
            this._secretsToolStripButton = new ImageButtonWithLabel();

            this._documentsToolStripButton = new ImageButtonWithLabel();
            this._oneDriveToolStripButton = new ImageButtonWithLabel();
            this._googleDriveToolStripButton = new ImageButtonWithLabel();
            this._dropBoxToolStripButton = new ImageButtonWithLabel();
            this._softwareStatusButton = new ImageButtonWithLabel();
            this._feedbackButton = new ImageButtonWithLabel();
            this._helpButton = new ImageButtonWithLabel();
            this._closeAndRemoveOpenFilesToolStripButton = new ImageButtonWithLabel();
            this._daysLeftPremiumLabel = new AxCrypt.Desktop.Window.PremiumLinkLabel();
            this._bottomCenterTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this._progressBackgroundWorker = new AxCrypt.Forms.Implementation.ProgressBackgroundComponent(this.components);
            this._notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this._notifyIconContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._notifyAdvancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._notifySignInToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._notifySignOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._notifyExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._bottomRightBusinessTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this._businessPrioritySupportToolStripMenuItem = new ImageButtonWithLabel();
            this._businessAddMoreUsersToolStripMenuItem = new ImageButtonWithLabel();
            this._backgroundImage = new System.Windows.Forms.PictureBox();

            this._recentFilesContextMenuStrip.SuspendLayout();
            this._progressContextMenuStrip.SuspendLayout();
            this._watchedFoldersContextMenuStrip.SuspendLayout();
            this._watchedFoldersTabPage.SuspendLayout();
            this._recentFilesTabPage.SuspendLayout();
            this._statusTabControl.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this._mainMenuStrip.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this._mainToolStripTableLayout.SuspendLayout();
            this._bottomCenterTableLayout.SuspendLayout();
            this._notifyIconContextMenuStrip.SuspendLayout();
            this.SuspendLayout();

            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(820, 100);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.tableLayoutPanel2.Controls.Add(this._mainMenuStrip, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(800, 24);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // _mainMenuStrip
            // 
            this._mainMenuStrip.BackColor = System.Drawing.ColorTranslator.FromHtml("#48772C"); 
            this._mainMenuStrip.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this._mainMenuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._debugToolStripMenuItem,
            this._helpToolStripMenuItem});
            this._mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this._mainMenuStrip.Name = "_mainMenuStrip";
            this._mainMenuStrip.Size = new System.Drawing.Size(167, 24);
            this._mainMenuStrip.TabIndex = 6;
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openEncryptedToolStripMenuItem,
            this._encryptToolStripMenuItem,
            this._decryptToolStripMenuItem,
            this._encryptedFoldersToolStripMenuItem,
            this._renameToolStripMenuItem,
            this._restoreAnonymousNamesToolStripMenuItem,
            this._addSecureFolderToolStripMenuItem,
            this._toolStripSeparator3,
            this._secureDeleteToolStripMenuItem,
            this._toolStripSeparator2,
            this._inviteUserToolStripMenuItem,
            this._optionsToolStripMenuItem,
            this._encryptionUpgradeMenuItem,
            this.toolStripSeparator2,
            this._cleanDecryptedToolStripMenuItem,
            this._signInToolStripMenuItem,
            this._signOutToolStripMenuItem,
            this._exitToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this._fileToolStripMenuItem.Text = "[&File]";
            // 
            // _openEncryptedToolStripMenuItem
            // 
            this._openEncryptedToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.open_encrypted;
            this._openEncryptedToolStripMenuItem.Name = "_openEncryptedToolStripMenuItem";
            this._openEncryptedToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._openEncryptedToolStripMenuItem.Text = "[&Open Encrypted]";
            // 
            // _encryptToolStripMenuItem
            // 
            this._encryptToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.encrypt;
            this._encryptToolStripMenuItem.Name = "_encryptToolStripMenuItem";
            this._encryptToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._encryptToolStripMenuItem.Text = "[&Encrypt]";
            // 
            // _decryptToolStripMenuItem
            // 
            this._decryptToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.decrypt;
            this._decryptToolStripMenuItem.Name = "_decryptToolStripMenuItem";
            this._decryptToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._decryptToolStripMenuItem.Text = "[&Decrypt]";
            // 
            // _encryptedFoldersToolStripMenuItem
            // 
            this._encryptedFoldersToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.folder;
            this._encryptedFoldersToolStripMenuItem.Name = "_encryptedFoldersToolStripMenuItem";
            this._encryptedFoldersToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._encryptedFoldersToolStripMenuItem.Text = "[Encrypted &Folders]";
            // 
            // _renameToolStripMenuItem
            // 
            this._renameToolStripMenuItem.Name = "_renameToolStripMenuItem";
            this._renameToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._renameToolStripMenuItem.Text = "[&Rename]";
            // 
            // _restoreAnonymousNamesToolStripMenuItem
            // 
            this._restoreAnonymousNamesToolStripMenuItem.Name = "_restoreAnonymousNamesToolStripMenuItem";
            this._restoreAnonymousNamesToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._restoreAnonymousNamesToolStripMenuItem.Text = "[&Restore Rename]";
            // 
            // _addSecureFolderToolStripMenuItem
            // 
            this._addSecureFolderToolStripMenuItem.Name = "_addSecureFolderToolStripMenuItem";
            this._addSecureFolderToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._addSecureFolderToolStripMenuItem.Text = "[Add &Secure Folder]";
            // 
            // _toolStripSeparator3
            // 
            this._toolStripSeparator3.Name = "_toolStripSeparator3";
            this._toolStripSeparator3.Size = new System.Drawing.Size(212, 6);
            // 
            // _secureDeleteToolStripMenuItem
            // 
            this._secureDeleteToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.delete;
            this._secureDeleteToolStripMenuItem.Name = "_secureDeleteToolStripMenuItem";
            this._secureDeleteToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._secureDeleteToolStripMenuItem.Text = "[&Secure Delete]";
            // 
            // _toolStripSeparator2
            // 
            this._toolStripSeparator2.Name = "_toolStripSeparator2";
            this._toolStripSeparator2.Size = new System.Drawing.Size(212, 6);
            // 
            // _inviteUserToolStripMenuItem
            // 
            this._inviteUserToolStripMenuItem.Name = "_inviteUserToolStripMenuItem";
            this._inviteUserToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._inviteUserToolStripMenuItem.Text = "[Invite]";
            // 
            // _optionsToolStripMenuItem
            // 
            this._optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._optionsLanguageToolStripMenuItem,
            this._optionsChangePassphraseToolStripMenuItem,
            this._passwordResetToolStripMenuItem,
            this._optionsEncryptionUpgradeModeToolStripMenuItem,
            this._alwaysOfflineToolStripMenuItem,
            this._optionsIncludeSubfoldersToolStripMenuItem,
            this._optionsHideRecentFilesToolStripMenuItem,
            this._inactivitySignOutToolStripMenuItem,
            this._optionsDebugToolStripMenuItem,
            this._optionsClearAllSettingsAndRestartToolStripMenuItem,
            this._keyManagementToolStripMenuItem});
            this._optionsToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.options;
            this._optionsToolStripMenuItem.Name = "_optionsToolStripMenuItem";
            this._optionsToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._optionsToolStripMenuItem.Text = "[O&ptions]";
            // 
            // _optionsLanguageToolStripMenuItem
            // 
            this._optionsLanguageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._englishLanguageToolStripMenuItem,
            this._germanLanguageToolStripMenuItem,
            this._dutchToolStripMenuItem,
            this._spanishLanguageToolStripMenuItem,
            this._francaisLanguageToolStripMenuItem,
            this._italianLanguageToolStripMenuItem,
            this._koreanLanguageToolStripMenuItem,
            this._polishLanguageToolStripMenuItem,
            this._portugueseBrazilToolStripMenuItem,
            this._russianLanguageToolStripMenuItem,
            this._swedishLanguageToolStripMenuItem,
            this._turkishLanguageToolStripMenuItem,
            this._chineseLanguageToolStripMenuItem});
            this._optionsLanguageToolStripMenuItem.Name = "_optionsLanguageToolStripMenuItem";
            this._optionsLanguageToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._optionsLanguageToolStripMenuItem.Text = "[&Language]";
            this._optionsLanguageToolStripMenuItem.DropDownOpening += new System.EventHandler(this.OptionsLanguageToolStripMenuItem_DropDownOpening);
            // 
            // _englishLanguageToolStripMenuItem
            // 
            this._englishLanguageToolStripMenuItem.Name = "_englishLanguageToolStripMenuItem";
            this._englishLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._englishLanguageToolStripMenuItem.Tag = "en-US";
            this._englishLanguageToolStripMenuItem.Text = "[English]";
            this._englishLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _germanLanguageToolStripMenuItem
            // 
            this._germanLanguageToolStripMenuItem.Name = "_germanLanguageToolStripMenuItem";
            this._germanLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._germanLanguageToolStripMenuItem.Tag = "de-DE";
            this._germanLanguageToolStripMenuItem.Text = "[Deutsch]";
            this._germanLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _dutchToolStripMenuItem
            // 
            this._dutchToolStripMenuItem.Name = "_dutchToolStripMenuItem";
            this._dutchToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._dutchToolStripMenuItem.Tag = "nl-NL";
            this._dutchToolStripMenuItem.Text = "[Dutch]";
            this._dutchToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _spanishLanguageToolStripMenuItem
            // 
            this._spanishLanguageToolStripMenuItem.Name = "_spanishLanguageToolStripMenuItem";
            this._spanishLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._spanishLanguageToolStripMenuItem.Tag = "es-ES";
            this._spanishLanguageToolStripMenuItem.Text = "[Español]";
            this._spanishLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _francaisLanguageToolStripMenuItem
            // 
            this._francaisLanguageToolStripMenuItem.Name = "_francaisLanguageToolStripMenuItem";
            this._francaisLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._francaisLanguageToolStripMenuItem.Tag = "fr-FR";
            this._francaisLanguageToolStripMenuItem.Text = "[Français]";
            this._francaisLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _italianLanguageToolStripMenuItem
            // 
            this._italianLanguageToolStripMenuItem.Name = "_italianLanguageToolStripMenuItem";
            this._italianLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._italianLanguageToolStripMenuItem.Tag = "it-IT";
            this._italianLanguageToolStripMenuItem.Text = "[Italiano]";
            this._italianLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _koreanLanguageToolStripMenuItem
            // 
            this._koreanLanguageToolStripMenuItem.Name = "_koreanLanguageToolStripMenuItem";
            this._koreanLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._koreanLanguageToolStripMenuItem.Tag = "ko";
            this._koreanLanguageToolStripMenuItem.Text = "[Korean]";
            this._koreanLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _polishLanguageToolStripMenuItem
            // 
            this._polishLanguageToolStripMenuItem.Name = "_polishLanguageToolStripMenuItem";
            this._polishLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._polishLanguageToolStripMenuItem.Tag = "pl-PL";
            this._polishLanguageToolStripMenuItem.Text = "[Polski]";
            this._polishLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _portugueseBrazilToolStripMenuItem
            // 
            this._portugueseBrazilToolStripMenuItem.Name = "_portugueseBrazilToolStripMenuItem";
            this._portugueseBrazilToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._portugueseBrazilToolStripMenuItem.Tag = "pt-BR";
            this._portugueseBrazilToolStripMenuItem.Text = "[Português (Brasil)]";
            this._portugueseBrazilToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _russianLanguageToolStripMenuItem
            // 
            this._russianLanguageToolStripMenuItem.Name = "_russianLanguageToolStripMenuItem";
            this._russianLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._russianLanguageToolStripMenuItem.Tag = "ru-RU";
            this._russianLanguageToolStripMenuItem.Text = "[русский]";
            this._russianLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _swedishLanguageToolStripMenuItem
            // 
            this._swedishLanguageToolStripMenuItem.Name = "_swedishLanguageToolStripMenuItem";
            this._swedishLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._swedishLanguageToolStripMenuItem.Tag = "sv-SE";
            this._swedishLanguageToolStripMenuItem.Text = "[Svenska]";
            this._swedishLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _turkishLanguageToolStripMenuItem
            // 
            this._turkishLanguageToolStripMenuItem.Name = "_turkishLanguageToolStripMenuItem";
            this._turkishLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._turkishLanguageToolStripMenuItem.Tag = "tr-TR";
            this._turkishLanguageToolStripMenuItem.Text = "[Türkçe]";
            this._turkishLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _chineseLanguageToolStripMenuItem
            // 
            this._chineseLanguageToolStripMenuItem.Name = "_chineseLanguageToolStripMenuItem";
            this._chineseLanguageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this._chineseLanguageToolStripMenuItem.Tag = "zh-CN";
            this._chineseLanguageToolStripMenuItem.Text = "[中文]";
            this._chineseLanguageToolStripMenuItem.Click += new System.EventHandler(this.LanguageToolStripMenuItem_ClickAsync);
            // 
            // _optionsChangePassphraseToolStripMenuItem
            // 
            this._optionsChangePassphraseToolStripMenuItem.Name = "_optionsChangePassphraseToolStripMenuItem";
            this._optionsChangePassphraseToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._optionsChangePassphraseToolStripMenuItem.Text = "[Change &Password]";
            // 
            // _passwordResetToolStripMenuItem
            // 
            this._passwordResetToolStripMenuItem.Name = "_passwordResetToolStripMenuItem";
            this._passwordResetToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._passwordResetToolStripMenuItem.Text = "[&Password Reset]";
            this._passwordResetToolStripMenuItem.Click += new System.EventHandler(this.PasswordReset_Click);
            // 
            // _optionsEncryptionUpgradeModeToolStripMenuItem
            // 
            this._optionsEncryptionUpgradeModeToolStripMenuItem.Name = "_optionsEncryptionUpgradeModeToolStripMenuItem";
            this._optionsEncryptionUpgradeModeToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._optionsEncryptionUpgradeModeToolStripMenuItem.Text = "[Auto Upgrade Encryption]";
            // 
            // _alwaysOfflineToolStripMenuItem
            // 
            this._alwaysOfflineToolStripMenuItem.Name = "_alwaysOfflineToolStripMenuItem";
            this._alwaysOfflineToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._alwaysOfflineToolStripMenuItem.Text = "[Always Offline]";
            // 
            // _optionsIncludeSubfoldersToolStripMenuItem
            // 
            this._optionsIncludeSubfoldersToolStripMenuItem.Name = "_optionsIncludeSubfoldersToolStripMenuItem";
            this._optionsIncludeSubfoldersToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._optionsIncludeSubfoldersToolStripMenuItem.Text = "[Include Subfolders]";
            // 
            // _inactivitySignOutToolStripMenuItem
            // 
            this._inactivitySignOutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._disableInactivitySignOutToolStripMenuItem,
            this._fiveMinuteInactivitySignOutToolStripMenuItem,
            this._fifteenMinuteInactivitySignOutToolStripMenuItem,
            this._thirtyMinuteInactivitySignOutToolStripMenuItem,
            this._sixtyMinuteInactivitySignOutToolStripMenuItem});
            this._inactivitySignOutToolStripMenuItem.Name = "_inactivitySignOutToolStripMenuItem";
            this._inactivitySignOutToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._inactivitySignOutToolStripMenuItem.Text = "[&Idle Sign Out]";
            this._inactivitySignOutToolStripMenuItem.DropDownOpening += new System.EventHandler(this.IdleSignOutToolStripMenuItem_DropDownOpening);
            // 
            // _disableInactivitySignOutToolStripMenuItem
            // 
            this._disableInactivitySignOutToolStripMenuItem.Name = "_disableInactivitySignOutToolStripMenuItem";
            this._disableInactivitySignOutToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this._disableInactivitySignOutToolStripMenuItem.Tag = 0;
            this._disableInactivitySignOutToolStripMenuItem.Text = "[Disable]";
            this._disableInactivitySignOutToolStripMenuItem.Click += new System.EventHandler(this.IdleSignOutToolStripMenuItem_ClickAsync);
            // 
            // _fiveMinuteInactivitySignOutToolStripMenuItem
            // 
            this._fiveMinuteInactivitySignOutToolStripMenuItem.Name = "_fiveMinuteInactivitySignOutToolStripMenuItem";
            this._fiveMinuteInactivitySignOutToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this._fiveMinuteInactivitySignOutToolStripMenuItem.Tag = "5";
            this._fiveMinuteInactivitySignOutToolStripMenuItem.Text = "[5 Minutes]";
            this._fiveMinuteInactivitySignOutToolStripMenuItem.Click += new System.EventHandler(this.IdleSignOutToolStripMenuItem_ClickAsync);
            // 
            // _fifteenMinuteInactivitySignOutToolStripMenuItem
            // 
            this._fifteenMinuteInactivitySignOutToolStripMenuItem.Name = "_fifteenMinuteInactivitySignOutToolStripMenuItem";
            this._fifteenMinuteInactivitySignOutToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this._fifteenMinuteInactivitySignOutToolStripMenuItem.Tag = "15";
            this._fifteenMinuteInactivitySignOutToolStripMenuItem.Text = "[15 Minutes]";
            this._fifteenMinuteInactivitySignOutToolStripMenuItem.Click += new System.EventHandler(this.IdleSignOutToolStripMenuItem_ClickAsync);
            // 
            // _thirtyMinuteInactivitySignOutToolStripMenuItem
            // 
            this._thirtyMinuteInactivitySignOutToolStripMenuItem.Name = "_thirtyMinuteInactivitySignOutToolStripMenuItem";
            this._thirtyMinuteInactivitySignOutToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this._thirtyMinuteInactivitySignOutToolStripMenuItem.Tag = "30";
            this._thirtyMinuteInactivitySignOutToolStripMenuItem.Text = "[30 Minutes]";
            this._thirtyMinuteInactivitySignOutToolStripMenuItem.Click += new System.EventHandler(this.IdleSignOutToolStripMenuItem_ClickAsync);
            // 
            // _sixtyMinuteInactivitySignOutToolStripMenuItem
            // 
            this._sixtyMinuteInactivitySignOutToolStripMenuItem.Name = "_sixtyMinuteInactivitySignOutToolStripMenuItem";
            this._sixtyMinuteInactivitySignOutToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this._sixtyMinuteInactivitySignOutToolStripMenuItem.Tag = "60";
            this._sixtyMinuteInactivitySignOutToolStripMenuItem.Text = "[60 Minutes]";
            this._sixtyMinuteInactivitySignOutToolStripMenuItem.Click += new System.EventHandler(this.IdleSignOutToolStripMenuItem_ClickAsync);
            // 
            // _optionsDebugToolStripMenuItem
            // 
            this._optionsDebugToolStripMenuItem.Name = "_optionsDebugToolStripMenuItem";
            this._optionsDebugToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._optionsDebugToolStripMenuItem.Text = "[&Debug]";
            // 
            // _optionsHideRecentFilesToolStripMenuItem
            // 
            this._optionsHideRecentFilesToolStripMenuItem.Name = "_optionsHideRecentFilesToolStripMenuItem";
            this._optionsHideRecentFilesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._optionsHideRecentFilesToolStripMenuItem.Text = "[&Hide Recent Files]";
            // 
            // _optionsClearAllSettingsAndRestartToolStripMenuItem
            // 
            this._optionsClearAllSettingsAndRestartToolStripMenuItem.Name = "_optionsClearAllSettingsAndRestartToolStripMenuItem";
            this._optionsClearAllSettingsAndRestartToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._optionsClearAllSettingsAndRestartToolStripMenuItem.Text = "[&Clear All Settings and Exit]";
            // 
            // _keyManagementToolStripMenuItem
            // 
            this._keyManagementToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._importOthersSharingKeyToolStripMenuItem,
            this._exportSharingKeyToolStripMenuItem,
            this.toolStripSeparator3,
            this._importMyPrivateKeyToolStripMenuItem,
            this._exportMyPrivateKeyToolStripMenuItem,
            this.toolStripSeparator4,
            this._createAccountToolStripMenuItem});
            this._keyManagementToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.key_management;
            this._keyManagementToolStripMenuItem.Name = "_keyManagementToolStripMenuItem";
            this._keyManagementToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this._keyManagementToolStripMenuItem.Text = "[&Key Management]";
            // 
            // _importOthersSharingKeyToolStripMenuItem
            // 
            this._importOthersSharingKeyToolStripMenuItem.Name = "_importOthersSharingKeyToolStripMenuItem";
            this._importOthersSharingKeyToolStripMenuItem.Size = new System.Drawing.Size(312, 22);
            this._importOthersSharingKeyToolStripMenuItem.Text = "[&Import Someone\'s Public Sharing Key]";
            this._importOthersSharingKeyToolStripMenuItem.ToolTipText = "[Ask your contact to export and send his/her Public Sharing Key to you and import" +
    " it here. Then you can share encrypted files.]";
            this._importOthersSharingKeyToolStripMenuItem.Click += new System.EventHandler(this.ImportOthersSharingKeyToolStripMenuItem_Click);
            // 
            // _exportSharingKeyToolStripMenuItem
            // 
            this._exportSharingKeyToolStripMenuItem.Name = "_exportSharingKeyToolStripMenuItem";
            this._exportSharingKeyToolStripMenuItem.Size = new System.Drawing.Size(312, 22);
            this._exportSharingKeyToolStripMenuItem.Text = "[E&xport My Public Sharing Key]";
            this._exportSharingKeyToolStripMenuItem.ToolTipText = "[Send the exported Public Sharing Key to others you want to be able to share encr" +
    "ypted files with you.]";
            this._exportSharingKeyToolStripMenuItem.Click += new System.EventHandler(this.ExportMySharingKeyToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(309, 6);
            // 
            // _importMyPrivateKeyToolStripMenuItem
            // 
            this._importMyPrivateKeyToolStripMenuItem.Name = "_importMyPrivateKeyToolStripMenuItem";
            this._importMyPrivateKeyToolStripMenuItem.Size = new System.Drawing.Size(312, 22);
            this._importMyPrivateKeyToolStripMenuItem.Text = "[Import &Account Secret and Sharing Key Pair]";
            this._importMyPrivateKeyToolStripMenuItem.ToolTipText = "[To move an account, export the Secret and Sharing Key Pair and then import it he" +
    "re.]";
            this._importMyPrivateKeyToolStripMenuItem.Click += new System.EventHandler(this.ImportMyPrivateKeyToolStripMenuItem_Click);
            // 
            // _exportMyPrivateKeyToolStripMenuItem
            // 
            this._exportMyPrivateKeyToolStripMenuItem.Name = "_exportMyPrivateKeyToolStripMenuItem";
            this._exportMyPrivateKeyToolStripMenuItem.Size = new System.Drawing.Size(312, 22);
            this._exportMyPrivateKeyToolStripMenuItem.Text = "[Export Account &Secret and Sharing Key Pair]";
            this._exportMyPrivateKeyToolStripMenuItem.ToolTipText = "[To move an account, export the Secret and Sharing Key Pair and then import it el" +
    "sewhere.]";
            this._exportMyPrivateKeyToolStripMenuItem.Click += new System.EventHandler(this.ExportMyPrivateKeyToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(309, 6);
            // 
            // _createAccountToolStripMenuItem
            // 
            this._createAccountToolStripMenuItem.Name = "_createAccountToolStripMenuItem";
            this._createAccountToolStripMenuItem.Size = new System.Drawing.Size(312, 22);
            this._createAccountToolStripMenuItem.Text = "[Create &Account]";
            this._createAccountToolStripMenuItem.ToolTipText = "[Create an Account with a Secret and Sharing Key Pair so you can share encrypted " +
    "files with others.]";
            this._createAccountToolStripMenuItem.Click += new System.EventHandler(this.CreateAccountToolStripMenuItem_Click);
            // 
            // _encryptionUpgradeMenuItem
            // 
            this._encryptionUpgradeMenuItem.Name = "_encryptionUpgradeMenuItem";
            this._encryptionUpgradeMenuItem.Size = new System.Drawing.Size(215, 22);
            this._encryptionUpgradeMenuItem.Text = "[Upgrade AxCrypt 1.x Files]";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(212, 6);
            // 
            // _cleanDecryptedToolStripMenuItem
            // 
            this._cleanDecryptedToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.clean_broom_red;
            this._cleanDecryptedToolStripMenuItem.Name = "_cleanDecryptedToolStripMenuItem";
            this._cleanDecryptedToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._cleanDecryptedToolStripMenuItem.Text = "[&Clean Decrypted]";
            // 
            // _signInToolStripMenuItem
            // 
            this._signInToolStripMenuItem.Name = "_signInToolStripMenuItem";
            this._signInToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._signInToolStripMenuItem.Text = "[Sign &In]";
            // 
            // _signOutToolStripMenuItem
            // 
            this._signOutToolStripMenuItem.Name = "_signOutToolStripMenuItem";
            this._signOutToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._signOutToolStripMenuItem.Text = "[Sign &Out]";
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.exit;
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this._exitToolStripMenuItem.Text = "[E&xit]";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this._exitToolStripMenuItem_Click);
            // 
            // _debugToolStripMenuItem
            // 
            this._debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._debugCheckVersionNowToolStripMenuItem,
            this._debugOptionsToolStripMenuItem,
            this._debugCryptoPolicyToolStripMenuItem,
            this._debugLoggingToolStripMenuItem,
            this._debugManageAccountToolStripMenuItem,
            this._tryBrokenFileToolStripMenuItem,
            this._VerifyFileToolStripMenuItem,
            this._axcryptFileFormatCheckToolStripMenuItem,
            this._debugOpenReportToolStripMenuItem});
            this._debugToolStripMenuItem.Name = "_debugToolStripMenuItem";
            this._debugToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this._debugToolStripMenuItem.Text = "[&Debug]";
            // 
            // _debugCheckVersionNowToolStripMenuItem
            // 
            this._debugCheckVersionNowToolStripMenuItem.Name = "_debugCheckVersionNowToolStripMenuItem";
            this._debugCheckVersionNowToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._debugCheckVersionNowToolStripMenuItem.Text = "[&Check Version Now]";
            // 
            // _debugOptionsToolStripMenuItem
            // 
            this._debugOptionsToolStripMenuItem.Name = "_debugOptionsToolStripMenuItem";
            this._debugOptionsToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._debugOptionsToolStripMenuItem.Text = "[&Options]";
            this._debugOptionsToolStripMenuItem.Click += new System.EventHandler(this.SetOptionsToolStripMenuItem_Click);
            // 
            // _debugCryptoPolicyToolStripMenuItem
            // 
            this._debugCryptoPolicyToolStripMenuItem.Name = "_debugCryptoPolicyToolStripMenuItem";
            this._debugCryptoPolicyToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._debugCryptoPolicyToolStripMenuItem.Text = "[License &Policy]";
            this._debugCryptoPolicyToolStripMenuItem.Visible = false;
            // 
            // _debugLoggingToolStripMenuItem
            // 
            this._debugLoggingToolStripMenuItem.Name = "_debugLoggingToolStripMenuItem";
            this._debugLoggingToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._debugLoggingToolStripMenuItem.Text = "[&Logging]";
            this._debugLoggingToolStripMenuItem.Click += new System.EventHandler(this.loggingToolStripMenuItem_Click);
            // 
            // _debugManageAccountToolStripMenuItem
            // 
            this._debugManageAccountToolStripMenuItem.Name = "_debugManageAccountToolStripMenuItem";
            this._debugManageAccountToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._debugManageAccountToolStripMenuItem.Text = "[&Manage Account]";
            this._debugManageAccountToolStripMenuItem.Click += new System.EventHandler(this.ManageAccountToolStripMenuItem_Click);
            // 
            // _tryBrokenFileToolStripMenuItem
            // 
            this._tryBrokenFileToolStripMenuItem.Name = "_tryBrokenFileToolStripMenuItem";
            this._tryBrokenFileToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._tryBrokenFileToolStripMenuItem.Text = "[&Try Broken File]";
            // 
            // _VerifyFileToolStripMenuItem
            // 
            this._VerifyFileToolStripMenuItem.Name = "_VerifyFileToolStripMenuItem";
            this._VerifyFileToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._VerifyFileToolStripMenuItem.Text = "[&Verfiy File]";
            // 
            // _axcryptFileFormatCheckToolStripMenuItem
            // 
            this._axcryptFileFormatCheckToolStripMenuItem.Name = "_axcryptFileFormatCheckToolStripMenuItem";
            this._axcryptFileFormatCheckToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._axcryptFileFormatCheckToolStripMenuItem.Text = "[&AxCrypt File Format Check]";
            // 
            // _debugOpenReportToolStripMenuItem
            // 
            this._debugOpenReportToolStripMenuItem.Name = "_debugOpenReportToolStripMenuItem";
            this._debugOpenReportToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            this._debugOpenReportToolStripMenuItem.Text = "[&Open Error Report Snapshot]";
            // 
            // _helpToolStripMenuItem
            // 
            this._helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._helpViewHelpMenuItem,
            this._helpAboutToolStripMenuItem,
            this._checkForUpdateToolStripMenuItem,
            this._getPremiumToolStripMenuItem,
            this._manageAccountToolStripMenuItem});
            this._helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            this._helpToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this._helpToolStripMenuItem.Text = "[&Help]";
            // 
            // _helpViewHelpMenuItem
            // 
            this._helpViewHelpMenuItem.Name = "_helpViewHelpMenuItem";
            this._helpViewHelpMenuItem.Size = new System.Drawing.Size(173, 22);
            this._helpViewHelpMenuItem.Text = "&View Help";
            this._helpViewHelpMenuItem.Click += new System.EventHandler(this._viewHelpMenuItem_Click);
            // 
            // _helpAboutToolStripMenuItem
            // 
            this._helpAboutToolStripMenuItem.Name = "_helpAboutToolStripMenuItem";
            this._helpAboutToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this._helpAboutToolStripMenuItem.Text = "[&About]";
            this._helpAboutToolStripMenuItem.Click += new System.EventHandler(this._aboutToolStripMenuItem_Click);
            // 
            // _checkForUpdateToolStripMenuItem
            // 
            this._checkForUpdateToolStripMenuItem.Name = "_checkForUpdateToolStripMenuItem";
            this._checkForUpdateToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this._checkForUpdateToolStripMenuItem.Text = "[&Check for update]";
            // 
            // _getPremiumToolStripMenuItem
            // 
            this._getPremiumToolStripMenuItem.Name = "_getPremiumToolStripMenuItem";
            this._getPremiumToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this._getPremiumToolStripMenuItem.Text = "[&Click here to unlock full encryption features!]";
            // 
            // _manageAccountToolStripMenuItem
            // 
            this._manageAccountToolStripMenuItem.Name = "_manageAccountToolStripMenuItem";
            this._manageAccountToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this._manageAccountToolStripMenuItem.Text = "[&Manage Account]";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel3.Controls.Add(this._softwareStatusButton, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this._mainToolStripTableLayout, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this._closeAndRemoveOpenFilesToolStripButton, 2, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(5, 24);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(800, 90);
            this.tableLayoutPanel3.TabIndex = 7;
            // 
            // _softwareStatusButton
            // 
            this._softwareStatusButton.AutoSize = false;
            this._softwareStatusButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.bulb_green_40px;
            this._softwareStatusButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._softwareStatusButton.Margin = new System.Windows.Forms.Padding(10, 12, 0, 0);
            this._softwareStatusButton.Name = "_softwareStatusButton";
            this._softwareStatusButton.Size = new System.Drawing.Size(80, 50);
            this._softwareStatusButton.Text = "[Update Available]";
            // 
            // _mainToolStrip
            // 
            this._mainToolStripTableLayout.AllowDrop = true;
            this._mainToolStripTableLayout.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._mainToolStripTableLayout.AutoSize = false;
            this._mainToolStripTableLayout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this._mainToolStripTableLayout.Dock = System.Windows.Forms.DockStyle.None;
            this._mainToolStripTableLayout.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this._mainToolStripTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._mainToolStripTableLayout.ColumnCount = 10;
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.77F));
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.77F));
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.77F));
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.77F));
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.77F));
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.77F));
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.77F));
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.77F));
            this._mainToolStripTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.77F));
            this._mainToolStripTableLayout.Controls.AddRange(new System.Windows.Forms.TableLayoutPanel[] {
            this._openEncryptedToolStripButton,
            this._encryptToolStripButton,
            this._stopSecuringToolStripButton,
            this._keyShareToolStripButton,
            this._secretsToolStripButton,
            new System.Windows.Forms.TableLayoutPanel(),
            this._documentsToolStripButton,
            this._oneDriveToolStripButton,
            this._googleDriveToolStripButton,
            this._dropBoxToolStripButton });
            this._mainToolStripTableLayout.Name = "_mainToolStrip";
            this._mainToolStripTableLayout.RowCount = 1;
            this._mainToolStripTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._mainToolStripTableLayout.Size = new System.Drawing.Size(570, 70);
            this._mainToolStripTableLayout.TabIndex = 10;
            this._mainToolStripTableLayout.BackColor = System.Drawing.Color.Transparent;
            // 
            // _openEncryptedToolStripButton
            // 
            this._openEncryptedToolStripButton.AutoSize = false;
            this._openEncryptedToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.open_encrypted_80px;
            this._openEncryptedToolStripButton.ToolStripeBackColor = _ENABLEDTOOLSTRIPBACKCOLOR;
            this._openEncryptedToolStripButton.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._openEncryptedToolStripButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this._openEncryptedToolStripButton.Padding = new System.Windows.Forms.Padding(0);
            this._openEncryptedToolStripButton.Name = "_openEncryptedToolStripButton";
            this._openEncryptedToolStripButton.ToolTipText = "[Click to select documents to encrypt]";
            this._openEncryptedToolStripButton.Text = "[Open Secured]";
            // 
            // _encryptToolStripButton
            // 
            this._encryptToolStripButton.AutoSize = false;
            this._encryptToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.encrypt_white_40px;
            this._encryptToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._encryptToolStripButton.ToolStripeBackColor = _ENABLEDTOOLSTRIPBACKCOLOR;
            this._encryptToolStripButton.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._encryptToolStripButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this._encryptToolStripButton.Name = "_encryptToolStripButton";
            this._encryptToolStripButton.Size = new System.Drawing.Size(60, 60);
            this._encryptToolStripButton.ToolTipText = "[Click to select documents to encrypt]";
            this._encryptToolStripButton.Text = "[Secure]";
            // 
            // _stopSecuringToolStripButton
            // 
            this._stopSecuringToolStripButton.AutoSize = false;
            this._stopSecuringToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.minus_40px;
            this._stopSecuringToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._stopSecuringToolStripButton.ToolStripeBackColor = _ENABLEDTOOLSTRIPBACKCOLOR; 
            this._stopSecuringToolStripButton.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._stopSecuringToolStripButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this._stopSecuringToolStripButton.Name = "_stopSecuringToolStripButton";
            this._stopSecuringToolStripButton.Size = new System.Drawing.Size(80, 60);
            this._stopSecuringToolStripButton.ToolTipText = "[Click to select documents to decrypt]";
            this._stopSecuringToolStripButton.Text = "[Stop Securing]";
            // 
            // _keyShareToolStripButton
            // 
            this._keyShareToolStripButton.AutoSize = false;
            this._keyShareToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.share_border_80px;
            this._keyShareToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._keyShareToolStripButton.ToolStripeBackColor = _ENABLEDTOOLSTRIPBACKCOLOR;
            this._keyShareToolStripButton.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._keyShareToolStripButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this._keyShareToolStripButton.Name = "_keyShareToolStripButton";
            this._keyShareToolStripButton.Size = new System.Drawing.Size(80, 60);
            this._keyShareToolStripButton.ToolTipText = "[Click to select documents to decrypt]";
            this._keyShareToolStripButton.Text = "[Share Keys]";
            // 
            // _secretsToolStripButton
            // 
            this._secretsToolStripButton.AutoSize = false;
            this._secretsToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.passwords_80px;
            this._secretsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._secretsToolStripButton.ToolStripeBackColor = _ENABLEDTOOLSTRIPBACKCOLOR; 
            this._secretsToolStripButton.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._secretsToolStripButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this._secretsToolStripButton.Name = "_secretsToolStripButton";
            this._secretsToolStripButton.Size = new System.Drawing.Size(80, 60);
            this._secretsToolStripButton.ToolTipText = "[Click to open the Password Manager]";
            this._secretsToolStripButton.Text = "[Passwords]";
            // 
            // _documentsToolStripButton
            // 
            this._documentsToolStripButton.AutoSize = false;
            this._documentsToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.documents_white_40px;
            this._documentsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._documentsToolStripButton.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._documentsToolStripButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this._documentsToolStripButton.Name = "_documentsToolStripButton";
            this._documentsToolStripButton.Size = new System.Drawing.Size(80, 60);
            this._documentsToolStripButton.Text = "[Documents]";
            this._documentsToolStripButton.Visible = false;
            this._documentsToolStripButton.Enabled = false;
            // 
            // _oneDriveToolStripButton
            // 
            this._oneDriveToolStripButton.AutoSize = false;
            this._oneDriveToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.skydrive_40px;
            this._oneDriveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._oneDriveToolStripButton.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._oneDriveToolStripButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this._oneDriveToolStripButton.Name = "_oneDriveToolStripButton";
            this._oneDriveToolStripButton.Size = new System.Drawing.Size(80, 60);
            this._oneDriveToolStripButton.Text = "[One Drive]";
            this._oneDriveToolStripButton.Visible = false;
            this._oneDriveToolStripButton.Enabled = false;
            // 
            // _googleDriveToolStripButton
            // 
            this._googleDriveToolStripButton.AutoSize = false;
            this._googleDriveToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.google_drive_40px;
            this._googleDriveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._googleDriveToolStripButton.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._googleDriveToolStripButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this._googleDriveToolStripButton.Name = "_googleDriveToolStripButton";
            this._googleDriveToolStripButton.Size = new System.Drawing.Size(80, 60);
            this._googleDriveToolStripButton.Text = "[Google Drive]";
            this._googleDriveToolStripButton.Visible = false;
            this._googleDriveToolStripButton.Enabled = false;
            // 
            // _dropBoxToolStripButton
            // 
            this._dropBoxToolStripButton.AutoSize = false;
            this._dropBoxToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.dropbox_40px;
            this._dropBoxToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._dropBoxToolStripButton.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._dropBoxToolStripButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this._dropBoxToolStripButton.Name = "_dropBoxToolStripButton";
            this._dropBoxToolStripButton.Size = new System.Drawing.Size(80, 60);
            this._dropBoxToolStripButton.Text = "[DropBox]";
            this._dropBoxToolStripButton.Visible = false;
            this._dropBoxToolStripButton.Enabled = false;
            // 
            // _closeAndRemoveOpenFilesToolStripButton
            // 
            this._closeAndRemoveOpenFilesToolStripButton.AutoSize = false;
            this._closeAndRemoveOpenFilesToolStripButton.Visible = false;
            this._closeAndRemoveOpenFilesToolStripButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.broom_40px;
            this._closeAndRemoveOpenFilesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._closeAndRemoveOpenFilesToolStripButton.ToolStripeBackColor = System.Drawing.ColorTranslator.FromHtml("#494949");
            this._closeAndRemoveOpenFilesToolStripButton.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this._closeAndRemoveOpenFilesToolStripButton.Name = "_closeAndRemoveOpenFilesToolStripButton";
            this._closeAndRemoveOpenFilesToolStripButton.Size = new System.Drawing.Size(60, 80);
            this._closeAndRemoveOpenFilesToolStripButton.ToolTipText = "[Click to clean and remove any open decrypted files, but ensure they are not in u" +
    "se first.]";
            this._closeAndRemoveOpenFilesToolStripButton.Text = "[Clean Up]";
            // 
            // _statusTabControl
            // 
            this._statusTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._statusTabControl.Controls.Add(this._recentFilesTabPage);
            this._statusTabControl.Controls.Add(this._watchedFoldersTabPage);
            this._statusTabControl.Location = new System.Drawing.Point(0, 110);
            this._statusTabControl.Name = "_statusTabControl";
            this._statusTabControl.SelectedIndex = 0;
            this._statusTabControl.ShowToolTips = true;
            this._statusTabControl.Size = new System.Drawing.Size(840, 250);
            this._statusTabControl.TabIndex = 2;
            // 
            // _recentFilesTabPage
            // 
            this._recentFilesTabPage.Controls.Add(this._recentFilesListView);
            this._recentFilesTabPage.Location = new System.Drawing.Point(4, 22);
            this._recentFilesTabPage.Name = "_recentFilesTabPage";
            this._recentFilesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._recentFilesTabPage.Size = new System.Drawing.Size(830, 200);
            this._recentFilesTabPage.TabIndex = 2;
            this._recentFilesTabPage.Text = "[Recent Files]";
            this._recentFilesTabPage.UseVisualStyleBackColor = true;
            // 
            // _recentFilesListView
            // 
            this._recentFilesListView.AllowDrop = true;
            this._recentFilesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._decryptedFileColumnHeader,
            this._fileSizeColumnHeader,
            this._lastAccessedDateColumnHeader,
            this._encryptedPathColumnHeader,
            this._lastModifiedDateColumnHeader,
            this._cryptoNameColumnHeader});
            this._recentFilesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._recentFilesListView.FullRowSelect = true;
            this._recentFilesListView.Location = new System.Drawing.Point(3, 3);
            this._recentFilesListView.Name = "_recentFilesListView";
            this._recentFilesListView.ShowItemToolTips = true;
            this._recentFilesListView.Size = new System.Drawing.Size(814, 194);
            this._recentFilesListView.TabIndex = 0;
            this._recentFilesListView.UseCompatibleStateImageBehavior = false;
            this._recentFilesListView.View = System.Windows.Forms.View.Details;
            this._recentFilesListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            //
            //
            this._backgroundImage.BackgroundImage = Properties.Resources.recent_files_background;
            this._backgroundImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this._backgroundImage.Dock = System.Windows.Forms.DockStyle.Fill;
            // _decryptedFileColumnHeader
            // 
            this._decryptedFileColumnHeader.Text = "[Document]";
            this._decryptedFileColumnHeader.Width = 159;
            // 
            // _fileSizeColumnHeader
            // 
            this._fileSizeColumnHeader.Text = "[Size]";
            this._fileSizeColumnHeader.Width = 159;
            //
            // _lastAccessTimeColumnHeader
            // 
            this._lastAccessedDateColumnHeader.Text = "[Time]";
            this._lastAccessedDateColumnHeader.Width = 180;
            // 
            // _encryptedPathColumnHeader
            // 
            this._encryptedPathColumnHeader.Text = "[Encrypted]";
            this._encryptedPathColumnHeader.Width = 169;
            // 
            // _cryptoName
            // 
            this._cryptoNameColumnHeader.Text = "[Algorithm]";
            // 
            // _lastModifiedDateColumnHeader
            // 
            this._lastModifiedDateColumnHeader.Text = "[OriginalTime]";
            this._lastModifiedDateColumnHeader.Width = 149;
            //
            // _watchedFoldersTabPage
            // 
            this._watchedFoldersTabPage.Controls.Add(this._watchedFoldersListView);
            this._watchedFoldersTabPage.Location = new System.Drawing.Point(4, 22);
            this._watchedFoldersTabPage.Name = "_watchedFoldersTabPage";
            this._watchedFoldersTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._watchedFoldersTabPage.Size = new System.Drawing.Size(830, 200);
            this._watchedFoldersTabPage.TabIndex = 1;
            this._watchedFoldersTabPage.Text = "[Encrypted Folders]";
            this._watchedFoldersTabPage.UseVisualStyleBackColor = true;
            // 
            // _watchedFoldersListView
            // 
            this._watchedFoldersListView.AllowDrop = true;
            this._watchedFoldersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._watchedFolderColumnHeader});
            this._watchedFoldersListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._watchedFoldersListView.FullRowSelect = true;
            this._watchedFoldersListView.Location = new System.Drawing.Point(3, 3);
            this._watchedFoldersListView.Name = "_watchedFoldersListView";
            this._watchedFoldersListView.ShowItemToolTips = true;
            this._watchedFoldersListView.Size = new System.Drawing.Size(814, 194);
            this._watchedFoldersListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this._watchedFoldersListView.TabIndex = 2;
            this._watchedFoldersListView.UseCompatibleStateImageBehavior = false;
            this._watchedFoldersListView.View = System.Windows.Forms.View.Details;
            this._watchedFoldersListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            // 
            // _watchedFolderColumnHeader
            // 
            this._watchedFolderColumnHeader.Text = "[Folder]";
            this._watchedFolderColumnHeader.Width = 368;
            // 
            // _recentFilesContextMenuStrip
            // 
            this._recentFilesContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._recentFilesOpenToolStripMenuItem,
            this._removeRecentFileToolStripMenuItem,
            this._decryptAndRemoveFromListToolStripMenuItem,
            this._shareKeysToolStripMenuItem,
            this._recentFilesShowInFolderToolStripMenuItem,
            this._recentFilesRestoreAnonymousNamesMenuItem,
            this._clearRecentFilesToolStripMenuItem});
            this._recentFilesContextMenuStrip.Name = "RecentFilesContextMenu";
            this._recentFilesContextMenuStrip.Size = new System.Drawing.Size(335, 158);
            this._recentFilesContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this._recentFilesContextMenuStrip_Opening);
            // 
            // _recentFilesOpenToolStripMenuItem
            // 
            this._recentFilesOpenToolStripMenuItem.Name = "_recentFilesOpenToolStripMenuItem";
            this._recentFilesOpenToolStripMenuItem.Size = new System.Drawing.Size(334, 22);
            this._recentFilesOpenToolStripMenuItem.Text = "[&Open]";
            // 
            // _removeRecentFileToolStripMenuItem
            // 
            this._removeRecentFileToolStripMenuItem.Name = "_removeRecentFileToolStripMenuItem";
            this._removeRecentFileToolStripMenuItem.Size = new System.Drawing.Size(334, 22);
            this._removeRecentFileToolStripMenuItem.Text = "[&Remove from list without decrypting document]";
            // 
            // _decryptAndRemoveFromListToolStripMenuItem
            // 
            this._decryptAndRemoveFromListToolStripMenuItem.Name = "_decryptAndRemoveFromListToolStripMenuItem";
            this._decryptAndRemoveFromListToolStripMenuItem.Size = new System.Drawing.Size(334, 22);
            this._decryptAndRemoveFromListToolStripMenuItem.Text = "[&Decrypt and remove from list]";
            // 
            // _shareKeysToolStripMenuItem
            // 
            this._shareKeysToolStripMenuItem.Name = "_shareKeysToolStripMenuItem";
            this._shareKeysToolStripMenuItem.Size = new System.Drawing.Size(334, 22);
            this._shareKeysToolStripMenuItem.Text = "[&Share Keys]";
            // 
            // _recentFilesShowInFolderToolStripMenuItem
            // 
            this._recentFilesShowInFolderToolStripMenuItem.Name = "_recentFilesShowInFolderToolStripMenuItem";
            this._recentFilesShowInFolderToolStripMenuItem.Size = new System.Drawing.Size(334, 22);
            this._recentFilesShowInFolderToolStripMenuItem.Text = "[Show in &Folder]";
            // 
            // _recentFilesRestoreAnonymousNamesMenuItem
            // 
            this._recentFilesRestoreAnonymousNamesMenuItem.Name = "_recentFilesRestoreAnonymousNamesMenuItem";
            this._recentFilesRestoreAnonymousNamesMenuItem.Size = new System.Drawing.Size(334, 22);
            this._recentFilesRestoreAnonymousNamesMenuItem.Text = "[Restore Anonymous Names]";
            // 
            // _clearRecentFilesToolStripMenuItem
            // 
            this._clearRecentFilesToolStripMenuItem.Name = "_clearRecentFilesToolStripMenuItem";
            this._clearRecentFilesToolStripMenuItem.Size = new System.Drawing.Size(334, 22);
            this._clearRecentFilesToolStripMenuItem.Text = "[&Clear]";
            // 
            // _progressContextMenuStrip
            // 
            this._progressContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._progressContextCancelToolStripMenuItem});
            this._progressContextMenuStrip.Name = "ProgressContextMenu";
            this._progressContextMenuStrip.Size = new System.Drawing.Size(119, 26);
            // 
            // _progressContextCancelToolStripMenuItem
            // 
            this._progressContextCancelToolStripMenuItem.Name = "_progressContextCancelToolStripMenuItem";
            this._progressContextCancelToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this._progressContextCancelToolStripMenuItem.Text = "[Cancel]";
            this._progressContextCancelToolStripMenuItem.Click += new System.EventHandler(this.ProgressContextCancelToolStripMenuItem_Click);
            // 
            // _watchedFoldersContextMenuStrip
            // 
            this._watchedFoldersContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._watchedFoldersKeySharingMenuItem,
            this._watchedFoldersDecryptMenuItem,
            this._watchedFoldersdecryptTemporarilyMenuItem,
            this._watchedFoldersRemoveMenuItem,
            this._watchedFoldersOpenExplorerHereMenuItem,
            this._watchedFoldersAddSecureFolderMenuItem});
            this._watchedFoldersContextMenuStrip.Name = "watchedFoldersContextMenuStrip";
            this._watchedFoldersContextMenuStrip.Size = new System.Drawing.Size(335, 136);
            // 
            // _watchedFoldersKeySharingMenuItem
            // 
            this._watchedFoldersKeySharingMenuItem.Name = "_watchedFoldersKeySharingMenuItem";
            this._watchedFoldersKeySharingMenuItem.Size = new System.Drawing.Size(334, 22);
            this._watchedFoldersKeySharingMenuItem.Text = "[Key Sharing]";
            // 
            // _watchedFoldersDecryptMenuItem
            // 
            this._watchedFoldersDecryptMenuItem.Name = "_watchedFoldersDecryptMenuItem";
            this._watchedFoldersDecryptMenuItem.Size = new System.Drawing.Size(334, 22);
            this._watchedFoldersDecryptMenuItem.Text = "[&Decrypt Permanently]";
            // 
            // _watchedFoldersdecryptTemporarilyMenuItem
            // 
            this._watchedFoldersdecryptTemporarilyMenuItem.Name = "_watchedFoldersdecryptTemporarilyMenuItem";
            this._watchedFoldersdecryptTemporarilyMenuItem.Size = new System.Drawing.Size(334, 22);
            this._watchedFoldersdecryptTemporarilyMenuItem.Text = "[Decrypt &Temporarily]";
            // 
            // _watchedFoldersRemoveMenuItem
            // 
            this._watchedFoldersRemoveMenuItem.Name = "_watchedFoldersRemoveMenuItem";
            this._watchedFoldersRemoveMenuItem.Size = new System.Drawing.Size(334, 22);
            this._watchedFoldersRemoveMenuItem.Text = "[&Remove from list without decrypting document]";
            // 
            // _watchedFoldersOpenExplorerHereMenuItem
            // 
            this._watchedFoldersOpenExplorerHereMenuItem.Name = "_watchedFoldersOpenExplorerHereMenuItem";
            this._watchedFoldersOpenExplorerHereMenuItem.Size = new System.Drawing.Size(334, 22);
            this._watchedFoldersOpenExplorerHereMenuItem.Text = "[Open &Explorer Here]";
            // 
            // _watchedFoldersAddSecureFolderMenuItem
            // 
            this._watchedFoldersAddSecureFolderMenuItem.Name = "_watchedFoldersAddSecureFolderMenuItem";
            this._watchedFoldersAddSecureFolderMenuItem.Size = new System.Drawing.Size(334, 22);
            this._watchedFoldersAddSecureFolderMenuItem.Text = "[Add &Secure Folder]";
            // 
            // _progressTableLayoutPanel
            // 
            this._progressTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._progressTableLayoutPanel.ColumnCount = 3;
            this._progressTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45));
            this._progressTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this._progressTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this._progressTableLayoutPanel.Controls.Add(this.pictureBox1, 0, 0);
            this._progressTableLayoutPanel.Controls.Add(this._bottomCenterTableLayout, 1, 0);
            this._progressTableLayoutPanel.Controls.Add(this._daysLeftPremiumLabel, 2, 0);
            this._progressTableLayoutPanel.Controls.Add(this._bottomRightBusinessTableLayout, 2, 0);
            this._progressTableLayoutPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this._progressTableLayoutPanel.Location = new System.Drawing.Point(0, 360);
            this._progressTableLayoutPanel.Name = "_progressTableLayoutPanel";
            this._progressTableLayoutPanel.RowCount = 2;
            this._progressTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this._progressTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._progressTableLayoutPanel.Size = new System.Drawing.Size(830, 80);
            this._progressTableLayoutPanel.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.pictureBox1.Image = global::AxCrypt.Desktop.Window.Properties.Resources.logo_text_retina;
            this.pictureBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(25, 20, 0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(145, 33);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            // 
            // _bottomCenterTableLayout
            // 
            this._bottomCenterTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._bottomCenterTableLayout.ColumnCount = 2;
            this._bottomCenterTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this._bottomCenterTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this._bottomCenterTableLayout.Controls.Add(this._feedbackButton, 0, 0);
            this._bottomCenterTableLayout.Controls.Add(this._helpButton, 1, 0);
            this._bottomCenterTableLayout.Margin = new System.Windows.Forms.Padding(0);
            this._bottomCenterTableLayout.Name = "_bottomCenterTableLayout";
            this._bottomCenterTableLayout.RowCount = 1;
            this._bottomCenterTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._bottomCenterTableLayout.Size = new System.Drawing.Size(150, 60);
            this._bottomCenterTableLayout.TabIndex = 1;
            // 
            // _feedbackButton
            // 
            this._feedbackButton.AutoSize = false;
            this._feedbackButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.feedback_40px;
            this._feedbackButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._feedbackButton.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this._feedbackButton.Name = "_feedbackButton";
            this._feedbackButton.Size = new System.Drawing.Size(75, 50);
            this._feedbackButton.Text = "[Send feedback]";
            // 
            // _helpButton
            // 
            this._helpButton.AutoSize = false;
            this._helpButton.Image = global::AxCrypt.Desktop.Window.Properties.Resources.helpquestion_40px;
            this._helpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._helpButton.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this._helpButton.ButtonTitleMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this._helpButton.Name = "_helpButton";
            this._helpButton.Size = new System.Drawing.Size(75, 50);
            this._helpButton.Text = "[Help]";
            // 
            // _daysLeftPremiumLabel
            //
            this._daysLeftPremiumLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this._daysLeftPremiumLabel.AutoSize = true;
            this._daysLeftPremiumLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._daysLeftPremiumLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this._daysLeftPremiumLabel.LinkColor = System.Drawing.Color.White;
            this._daysLeftPremiumLabel.Name = "_daysLeftPremiumLabel";
            this._daysLeftPremiumLabel.MinimumSize = new System.Drawing.Size(60, 40);
            this._daysLeftPremiumLabel.MaximumSize = new System.Drawing.Size(200, 40);
            this._daysLeftPremiumLabel.TabIndex = 11;
            this._daysLeftPremiumLabel.TabStop = true;
            this._daysLeftPremiumLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this._daysLeftPremiumLabel.Text = "[&Click here to unlock full encryption features!]";
            this._daysLeftPremiumLabel.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this._daysLeftPremiumLabel.Margin = new System.Windows.Forms.Padding(0, 18, 30, 0);
            this._daysLeftPremiumLabel.BackColor = System.Drawing.Color.FromArgb(134, 185, 110);
            this._daysLeftPremiumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._daysLeftPremiumLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._daysLeftPremiumLabel.Image = global::AxCrypt.Desktop.Window.Properties.Resources.premium_overlay_16px;
            this._daysLeftPremiumLabel.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // _bottomRightBusinessTableLayout
            // 
            this._bottomRightBusinessTableLayout.Visible = false;
            this._bottomRightBusinessTableLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top) | System.Windows.Forms.AnchorStyles.Right)));
            this._bottomRightBusinessTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._bottomRightBusinessTableLayout.ColumnCount = 2;
            this._bottomRightBusinessTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this._bottomRightBusinessTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this._bottomRightBusinessTableLayout.Controls.Add(this._businessAddMoreUsersToolStripMenuItem, 0, 0);
            this._bottomRightBusinessTableLayout.Controls.Add(this._businessPrioritySupportToolStripMenuItem, 1, 0);
            this._bottomRightBusinessTableLayout.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this._bottomRightBusinessTableLayout.Name = "_bottomRightBusinessTableLayout";
            this._bottomRightBusinessTableLayout.RowCount = 1;
            this._bottomRightBusinessTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this._bottomRightBusinessTableLayout.Size = new System.Drawing.Size(160, 80);
            this._bottomRightBusinessTableLayout.TabIndex = 1;
            // 
            // _businessAddMoreUsersToolStripMenuItem
            // 
            this._businessAddMoreUsersToolStripMenuItem.AutoSize = false;
            this._businessAddMoreUsersToolStripMenuItem.Visible = false;
            this._businessAddMoreUsersToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.add_more_users_white;
            this._businessAddMoreUsersToolStripMenuItem.ToolStripeBackColor = System.Drawing.Color.FromArgb(27, 39, 32);
            this._businessAddMoreUsersToolStripMenuItem.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._businessAddMoreUsersToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this._businessAddMoreUsersToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._businessAddMoreUsersToolStripMenuItem.Name = "_businessAddMoreUsersToolStripMenuItem";
            this._businessAddMoreUsersToolStripMenuItem.Text = "[Add more users]";
            // 
            // _businessPrioritySupportToolStripMenuItem
            // 
            this._businessPrioritySupportToolStripMenuItem.AutoSize = false;
            this._businessPrioritySupportToolStripMenuItem.Visible = false;
            this._businessPrioritySupportToolStripMenuItem.Image = global::AxCrypt.Desktop.Window.Properties.Resources.business_support_white;
            this._businessPrioritySupportToolStripMenuItem.ToolStripeBackColor = System.Drawing.Color.FromArgb(27, 39, 32);
            this._businessPrioritySupportToolStripMenuItem.ButtonTitleMargin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._businessPrioritySupportToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this._businessPrioritySupportToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._businessPrioritySupportToolStripMenuItem.Name = "_businessPrioritySupportToolStripMenuItem";
            this._businessPrioritySupportToolStripMenuItem.ToolTipText = "[Get quick and personal support, with a dedicated account manager]";
            this._businessPrioritySupportToolStripMenuItem.Text = "[Business Support]";
            // 
            // _progressBackgroundWorker
            // 
            this._progressBackgroundWorker.ProgressBarCreated += new System.EventHandler<System.Windows.Forms.ControlEventArgs>(this.ProgressBackgroundWorker_ProgressBarCreated);
            this._progressBackgroundWorker.ProgressBarClicked += new System.EventHandler<System.Windows.Forms.MouseEventArgs>(this.ProgressBackgroundWorker_ProgressBarClicked);
            // 
            // _notifyIcon
            // 
            this._notifyIcon.ContextMenuStrip = this._notifyIconContextMenuStrip;
            this._notifyIcon.Text = "[AxCrypt]";
            this._notifyIcon.Visible = true;
            // 
            // _notifyIconContextMenuStrip
            // 
            this._notifyIconContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._notifyAdvancedToolStripMenuItem,
            this._notifySignInToolStripMenuItem,
            this._notifySignOutToolStripMenuItem,
            this._notifyExitToolStripMenuItem});
            this._notifyIconContextMenuStrip.Name = "_notifyIconContextMenuStrip";
            this._notifyIconContextMenuStrip.Size = new System.Drawing.Size(136, 92);
            // 
            // _notifyAdvancedToolStripMenuItem
            // 
            this._notifyAdvancedToolStripMenuItem.Name = "_notifyAdvancedToolStripMenuItem";
            this._notifyAdvancedToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this._notifyAdvancedToolStripMenuItem.Text = "[Advanced]";
            // 
            // _notifySignInToolStripMenuItem
            // 
            this._notifySignInToolStripMenuItem.Name = "_notifySignInToolStripMenuItem";
            this._notifySignInToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this._notifySignInToolStripMenuItem.Text = "[Sign In]";
            // 
            // _notifySignOutToolStripMenuItem
            // 
            this._notifySignOutToolStripMenuItem.Name = "_notifySignOutToolStripMenuItem";
            this._notifySignOutToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this._notifySignOutToolStripMenuItem.Text = "[Sign Out]";
            // 
            // _notifyExitToolStripMenuItem
            // 
            this._notifyExitToolStripMenuItem.Name = "_notifyExitToolStripMenuItem";
            this._notifyExitToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this._notifyExitToolStripMenuItem.Text = "[&Exit]";
            this._notifyExitToolStripMenuItem.Click += new System.EventHandler(this._exitToolStripMenuItem_Click);
            // 
            // AxCryptMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 435);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this._statusTabControl);
            this.Controls.Add(this._progressTableLayoutPanel);
            this.BackColor = System.Drawing.ColorTranslator.FromHtml("#48772C");
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(840, 460);
            this.Name = "AxCryptMainForm";
            this.Text = "[AxCrypt]";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AxCryptMainForm_FormClosing);
            this.Load += new System.EventHandler(this.AxCryptMainForm_Load);
            this.Shown += new System.EventHandler(this.AxCryptMainForm_ShownAsync);
            this._recentFilesContextMenuStrip.ResumeLayout(false);
            this._progressContextMenuStrip.ResumeLayout(false);
            this._watchedFoldersContextMenuStrip.ResumeLayout(false);
            this._watchedFoldersTabPage.ResumeLayout(false);
            this._recentFilesTabPage.ResumeLayout(false);
            this._statusTabControl.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this._mainMenuStrip.ResumeLayout(false);
            this._mainMenuStrip.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this._mainToolStripTableLayout.ResumeLayout(false);
            this._mainToolStripTableLayout.PerformLayout();
            this._bottomCenterTableLayout.ResumeLayout(false);
            this._bottomCenterTableLayout.PerformLayout();
            this._notifyIconContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip _recentFilesContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem _removeRecentFileToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel _progressTableLayoutPanel;
        private System.Windows.Forms.ContextMenuStrip _progressContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem _progressContextCancelToolStripMenuItem;
        private AxCrypt.Forms.Implementation.ProgressBackgroundComponent _progressBackgroundWorker;
        private System.Windows.Forms.ToolStripMenuItem _decryptAndRemoveFromListToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip _watchedFoldersContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem _watchedFoldersDecryptMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _watchedFoldersdecryptTemporarilyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _watchedFoldersRemoveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _watchedFoldersOpenExplorerHereMenuItem;
        private System.Windows.Forms.TabPage _watchedFoldersTabPage;
        private System.Windows.Forms.ListView _watchedFoldersListView;
        private System.Windows.Forms.ColumnHeader _watchedFolderColumnHeader;
        private System.Windows.Forms.TabPage _recentFilesTabPage;
        private RecentFilesListView _recentFilesListView;
        private System.Windows.Forms.ColumnHeader _decryptedFileColumnHeader;
        private System.Windows.Forms.ColumnHeader _fileSizeColumnHeader;
        private System.Windows.Forms.ColumnHeader _lastAccessedDateColumnHeader;
        private System.Windows.Forms.ColumnHeader _lastModifiedDateColumnHeader;
        private System.Windows.Forms.ColumnHeader _encryptedPathColumnHeader;
        private System.Windows.Forms.ColumnHeader _cryptoNameColumnHeader;
        private System.Windows.Forms.TabControl _statusTabControl;
        private System.Windows.Forms.ToolStripMenuItem _shareKeysToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _restoreAnonymousNamesToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.MenuStrip _mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _openEncryptedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _encryptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _decryptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _encryptedFoldersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _inviteUserToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _cleanDecryptedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem _secureDeleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _optionsLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _englishLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _swedishLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _optionsChangePassphraseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _optionsDebugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _optionsHideRecentFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _optionsClearAllSettingsAndRestartToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _debugCheckVersionNowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _debugOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _debugCryptoPolicyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _debugLoggingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _debugManageAccountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _VerifyFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _axcryptFileFormatCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpViewHelpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpAboutToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel _mainToolStripTableLayout;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TableLayoutPanel _bottomCenterTableLayout;
        private System.Windows.Forms.ToolStripMenuItem _recentFilesOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _tryBrokenFileToolStripMenuItem;

        private ImageButtonWithLabel _softwareStatusButton;
        private ImageButtonWithLabel _openEncryptedToolStripButton;
        private ImageButtonWithLabel _encryptToolStripButton;
        private ImageButtonWithLabel _stopSecuringToolStripButton;
        private ImageButtonWithLabel _keyShareToolStripButton;
        private ImageButtonWithLabel _secretsToolStripButton;
        private ImageButtonWithLabel _documentsToolStripButton;
        private ImageButtonWithLabel _oneDriveToolStripButton;
        private ImageButtonWithLabel _googleDriveToolStripButton;
        private ImageButtonWithLabel _dropBoxToolStripButton;
        private ImageButtonWithLabel _closeAndRemoveOpenFilesToolStripButton;
        private ImageButtonWithLabel _helpButton;
        private ImageButtonWithLabel _feedbackButton;
        private PremiumLinkLabel _daysLeftPremiumLabel;

        private System.Windows.Forms.ToolStripMenuItem _signInToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _signOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _francaisLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _watchedFoldersAddSecureFolderMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _addSecureFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _watchedFoldersKeySharingMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _spanishLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _optionsEncryptionUpgradeModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _debugOpenReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _recentFilesRestoreAnonymousNamesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _encryptionUpgradeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _germanLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _koreanLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _italianLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _portugueseBrazilToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _dutchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _alwaysOfflineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _checkForUpdateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _getPremiumToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _passwordResetToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private System.Windows.Forms.ContextMenuStrip _notifyIconContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem _notifyAdvancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _notifySignOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _notifyExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _notifySignInToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _recentFilesShowInFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _clearRecentFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _turkishLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _chineseLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _optionsIncludeSubfoldersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _inactivitySignOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _disableInactivitySignOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _fiveMinuteInactivitySignOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _fifteenMinuteInactivitySignOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _thirtyMinuteInactivitySignOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _sixtyMinuteInactivitySignOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _russianLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _polishLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _keyManagementToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _importOthersSharingKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exportSharingKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem _importMyPrivateKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exportMyPrivateKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem _createAccountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _manageAccountToolStripMenuItem;

        private System.Windows.Forms.TableLayoutPanel _bottomRightBusinessTableLayout;
        private ImageButtonWithLabel _businessPrioritySupportToolStripMenuItem;
        private ImageButtonWithLabel _businessAddMoreUsersToolStripMenuItem;
        private System.Windows.Forms.PictureBox _backgroundImage;
    }
}
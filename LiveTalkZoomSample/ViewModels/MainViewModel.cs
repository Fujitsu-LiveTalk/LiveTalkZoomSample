/*
 * Copyright 2020 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * クラス名　：MainViewModel
 * 概要      ：MainViewModel
*/
using LiveTalkZoomSample.Common;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace LiveTalkZoomSample.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Models.ZoomModel Model = new Models.ZoomModel();
        private LiveTalk.FileCollaboration FileInterface = null;
        private SynchronizationContext Context = SynchronizationContext.Current;

        /// <summary>
        /// APIトークン
        /// </summary>
        public string APIToken
        {
            get { return this.Model.APIToken; }
            set { this.Model.APIToken = value; }
        }

        /// <summary>
        /// 連携ファイル名
        /// </summary>
        public string FileName
        {
            get { return this.Model.FileName; }
            set { this.Model.FileName = value; }
        }

        /// <summary>
        /// 処理メッセージ
        /// </summary>
        public string Message
        {
            get { return this.Model.Message; }
            set { this.Model.Message = value; }
        }

        /// <summary>
        /// true:発言者付き字幕
        /// </summary>
        public bool IsWithName
        {
            get { return this.Model.IsWithName; }
            set { this.Model.IsWithName = value; }
        }

        /// <summary>
        /// 認証PROXYならIDを指定
        /// </summary>
        public string ProxyId
        {
            get { return this.Model.ProxyId; }
            set { this.Model.ProxyId = value; }
        }

        /// <summary>
        /// 認証PROXYならパスワードを指定
        /// </summary>
        public string ProxyPassword
        {
            get { return this.Model.ProxyPassword; }
            set { this.Model.ProxyPassword = value; }
        }

        /// <summary>
        /// True:連携開始可能
        /// </summary>
        private bool _IsCanStart = false;
        public bool IsCanStart
        {
            get { return this._IsCanStart; }
            set
            {
                if (this._IsCanStart != value)
                {
                    this._IsCanStart = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// True:連携中フラグ
        /// </summary>
        private bool _IsStarted = false;
        public bool IsStarted
        {
            get { return this._IsStarted; }
            set
            {
                if (this._IsStarted != value)
                {
                    this._IsStarted = value;
                    OnPropertyChanged();
                    this.SetCanStartFlag();
                }
            }
        }
        private void SetCanStartFlag()
        {
            this.IsCanStart = !string.IsNullOrEmpty(this.APIToken) && !string.IsNullOrEmpty(this.FileName) && !this.IsStarted;
        }

        /// <summary>
        /// True:高さ最大
        /// </summary>
        private bool _IsHighHeight = true;
        public bool IsHighHeight
        {
            get { return this._IsHighHeight; }
            set
            {
                if (this._IsHighHeight != value)
                {
                    this._IsHighHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            this.Model.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(e.PropertyName);
                if (e.PropertyName == "APIToken" || e.PropertyName== "FileName")
                {
                    this.SetCanStartFlag();
                }
            };
            this.SetCanStartFlag();
        }

        /// <summary>
        /// 常時ファイル入力
        /// </summary>
        RelayCommand _SharedInputCommand;
        public RelayCommand SharedInputCommand
        {
            get
            {
                if (_SharedInputCommand == null)
                {
                    _SharedInputCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            var dialog = new Microsoft.Win32.SaveFileDialog
                            {
                                FilterIndex = 1,
                                Filter = "連携ファイル(*.csv)|*.csv",
                                Title = "連携ファイル名を指定",
                                CreatePrompt = true,
                                OverwritePrompt = false,
                                DefaultExt = "csv"
                            };
                            if (string.IsNullOrEmpty(this.FileName))
                            {
                                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                                dialog.FileName = "Output.csv";
                            }
                            else
                            {
                                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(this.FileName);
                                dialog.FileName = System.IO.Path.GetFileName(this.FileName);
                            }
                            if (dialog.ShowDialog() == true)
                            {
                                this.FileName = dialog.FileName;
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    });
                }
                return _SharedInputCommand;
            }
            set
            {
                _SharedInputCommand = value;
            }
        }

        /// <summary>
        /// 字幕送信開始
        /// </summary>
        RelayCommand _StartCommand;
        public RelayCommand StartCommand
        {
            get
            {
                if (_StartCommand == null)
                {
                    _StartCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            this.Start();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    });
                }
                return _StartCommand;
            }
            set
            {
                _StartCommand = value;
            }
        }
        private void Start()
        {
            // 連携ファイルをクリア
            if (System.IO.File.Exists(this.FileName))
            {
                System.IO.File.Delete(this.FileName);
            }

            // ファイル更新検出時の処理
            this.FileInterface = new LiveTalk.FileCollaboration(this.FileName, string.Empty);
            this.FileInterface.RemoteMessageReceived += ((s) =>
            {
                try
                {
                    this.Model.SendMessage(s);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            //　ファイル更新検出開始
            this.FileInterface.WatchFileSart();
            this.IsStarted = true;
        }

        /// <summary>
        /// 字幕送信終了
        /// </summary>
        RelayCommand _StopCommand;
        public RelayCommand StopCommand
        {
            get
            {
                if (_StopCommand == null)
                {
                    _StopCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            this.Stop();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    });
                }
                return _StopCommand;
            }
            set
            {
                _StopCommand = value;
            }
        }
        private void Stop()
        {
            // ファイル更新検出終了
            this.FileInterface?.WatchFileStop();
            this.IsStarted = false;
        }

        /// <summary>
        /// 画面に設定項目を表示する
        /// </summary>
        RelayCommand _HighHeightCommand;
        public RelayCommand HighHeightCommand
        {
            get
            {
                if (_HighHeightCommand == null)
                {
                    _HighHeightCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            this.IsHighHeight = true;
                        }
                        catch { }
                    });
                }
                return _HighHeightCommand;
            }
            set
            {
                _HighHeightCommand = value;
            }
        }

        /// <summary>
        /// 画面から設定項目を隠す
        /// </summary>
        RelayCommand _LowHeightCommand;
        public RelayCommand LowHeightCommand
        {
            get
            {
                if (_LowHeightCommand == null)
                {
                    _LowHeightCommand = new RelayCommand(() =>
                    {
                        try
                        {
                            this.IsHighHeight = false;
                        }
                        catch { }
                    });
                }
                return _LowHeightCommand;
            }
            set
            {
                _LowHeightCommand = value;
            }
        }

        /// <summary>
        /// APIトークンのクリア
        /// </summary>
        RelayCommand _ClearCommand;
        public RelayCommand ClearCommand
        {
            get
            {
                if (_ClearCommand == null)
                {
                    _ClearCommand = new RelayCommand(() =>
                    {
                        this.APIToken = Clipboard.GetText();
                    });
                }
                return _ClearCommand;
            }
            set
            {
                _ClearCommand = value;
            }
        }

        /// <summary>
        /// 画面クローズ
        /// </summary>
        RelayCommand _ExitCommand;
        public RelayCommand ExitCommand
        {
            get
            {
                if (_ExitCommand == null)
                {
                    _ExitCommand = new RelayCommand(() =>
                    {
                        OnClosed();
                    });
                }
                return _ExitCommand;
            }
            set
            {
                _ExitCommand = value;
            }
        }

        public event EventHandler Closed;
        protected virtual void OnClosed()
        {
            this.Closed?.Invoke(this, new EventArgs());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]String propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

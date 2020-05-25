/*
 * Copyright 2020 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * クラス名　：MainViewModel
 * 概要      ：MainViewModel
*/
using LiveTalkZoomSample.Common;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace LiveTalkZoomSample.ViewModels
{
    public class MainViewModel : IDisposable
    {
        private Models.ZoomModel Model = new Models.ZoomModel();
        private LiveTalk.FileCollaboration FileInterface = null;

        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        #region "ZoomModel-Property"
        [Required]       // 必須チェック
        public ReactiveProperty<string> APIToken { get; }
        [Required]       // 必須チェック
        public ReactiveProperty<string> FileName { get; }
        public ReactiveProperty<string> Message { get; }
        public ReactiveProperty<bool> IsWithName { get; }
        public ReactiveProperty<string> ProxyId { get; }
        public ReactiveProperty<string> ProxyPassword { get; }
        #endregion

        /// <summary>
        /// True:連携開始可能
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsCanStart { get; }

        /// <summary>
        /// True:連携中フラグ
        /// </summary>
        public ReactiveProperty<bool> IsStarted { get; } = new ReactiveProperty<bool>();

        /// <summary>
        /// True:高さ最大
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsHighHeight { get; }

        public MainViewModel()
        {
            // プロパティ設定
            this.APIToken = this.Model.ToReactivePropertyAsSynchronized((x) => x.APIToken)
                .SetValidateAttribute(() => this.APIToken)
                .AddTo(this.Disposable);
            this.FileName = this.Model.ToReactivePropertyAsSynchronized((x) => x.FileName)
                .SetValidateAttribute(() => this.FileName)
                .AddTo(this.Disposable);
            this.Message = this.Model.ToReactivePropertyAsSynchronized((x) => x.Message)
                .AddTo(this.Disposable);
            this.IsWithName = this.Model.ToReactivePropertyAsSynchronized((x) => x.IsWithName)
                .AddTo(this.Disposable);
            this.ProxyId = this.Model.ToReactivePropertyAsSynchronized((x) => x.ProxyId)
                .AddTo(this.Disposable);
            this.ProxyPassword = this.Model.ToReactivePropertyAsSynchronized((x) => x.ProxyPassword)
                .AddTo(this.Disposable);

            // 3つのステータスがすべてFalseの時だけスタートボタンがクリックできる
            this.IsCanStart = new[]
            {
                this.APIToken.ObserveHasErrors,
                this.FileName.ObserveHasErrors,
                this.IsStarted,
            }.CombineLatestValuesAreAllFalse()
             .ToReadOnlyReactiveProperty()
             .AddTo(this.Disposable);

            // コマンドによりOn/Off制御
            this.IsHighHeight = this.HighHeightCommand
                .Select(_ => true)
                .Merge(this.LowHeightCommand.Select(_ => false))
                .ToReadOnlyReactiveProperty(initialValue: true)
                .AddTo(this.Disposable);

            // コマンド設定
            this.SharedInputCommand = this.IsStarted.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(() => this.SharedInput())
                .AddTo(this.Disposable);
            this.StartCommand = this.IsCanStart
                .ToReactiveCommand()
                .WithSubscribe(() => this.Start())
                .AddTo(this.Disposable);
            this.StopCommand = this.IsStarted
                .ToReactiveCommand()
                .WithSubscribe(() => this.Stop())
                .AddTo(this.Disposable);
            this.ClearCommand = this.IsStarted.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(() => this.APIToken.Value = Clipboard.GetText())
                .AddTo(this.Disposable);
            this.ExitCommand.Subscribe((x) =>
            {
                OnClosed();
            }).AddTo(this.Disposable);

            // エラーハンドリング
            this.Model.Threw += (s, e) =>
            {
                MessageBox.Show(e.GetException().Message, "LiveTalk Zoom Subtitles Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            };
        }

        /// <summary>
        /// 常時ファイル入力
        /// </summary>
        public ReactiveCommand SharedInputCommand { get; }
        private void SharedInput()
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
                if (string.IsNullOrEmpty(this.FileName.Value))
                {
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    dialog.FileName = "Output.csv";
                }
                else
                {
                    dialog.InitialDirectory = System.IO.Path.GetDirectoryName(this.FileName.Value);
                    dialog.FileName = System.IO.Path.GetFileName(this.FileName.Value);
                }
                if (dialog.ShowDialog() == true)
                {
                    this.FileName.Value = dialog.FileName;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 字幕送信開始
        /// </summary>
        public ReactiveCommand StartCommand { get; }
        private void Start()
        {
            try
            {
                // 連携ファイルをクリア
                if (System.IO.File.Exists(this.FileName.Value))
                {
                    System.IO.File.Delete(this.FileName.Value);
                }

                // ファイル更新検出時の処理
                this.FileInterface = new LiveTalk.FileCollaboration(this.FileName.Value, string.Empty);
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
                this.IsStarted.Value = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 字幕送信終了
        /// </summary>
        public ReactiveCommand StopCommand { get; }
        private void Stop()
        {
            try
            {
                // ファイル更新検出終了
                this.FileInterface?.WatchFileStop();
                this.IsStarted.Value = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 画面に設定項目を表示する
        /// </summary>
        public ReactiveCommand HighHeightCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 画面から設定項目を隠す
        /// </summary>
        public ReactiveCommand LowHeightCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// APIトークンのコピー
        /// </summary>
        public ReactiveCommand ClearCommand { get; }

        /// <summary>
        /// 画面クローズ
        /// </summary>
        public ReactiveCommand ExitCommand { get; } = new ReactiveCommand();

        public event EventHandler Closed;
        protected virtual void OnClosed()
        {
            this.Closed?.Invoke(this, new EventArgs());
        }

        public void Dispose()
        {
            this.Disposable.Dispose();
        }
    }
}

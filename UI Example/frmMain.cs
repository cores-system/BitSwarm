﻿using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using SuRGeoNix.TorSwarm;
using SuRGeoNix;

namespace UI_Example
{
    public partial class frmMain : Form
    {
        Torrent                 torrent;
        TorSwarm                tr;
        TorSwarm.OptionsStruct  opt;

        public frmMain()
        {
            InitializeComponent();
        }
        
        private void frmMain_Load(object sender, EventArgs e)
        {
            TorSwarm.OptionsStruct opt = TorSwarm.GetDefaultsOptions();

            downPath.Text           = opt.DownloadPath;
            maxCon.Text             = opt.MaxConnections.ToString();
            minThreads.Text         = opt.MinThreads.ToString();
            peersFromTrackers.Text  = opt.PeersFromTracker.ToString();
            conTimeout.Text         = opt.ConnectionTimeout.ToString();
            handTimeout.Text        = opt.HandshakeTimeout.ToString();
            pieceTimeout.Text       = opt.PieceTimeout.ToString();
            metaTimeout.Text        = opt.MetadataTimeout.ToString();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if ( button1.Text == "Start" )
            {
                output.Text = "";

                try
                {
                    opt = TorSwarm.GetDefaultsOptions();
                    
                    opt.DownloadPath        = downPath.Text;

                    opt.MaxConnections      = int.Parse(maxCon.Text);
                    opt.MinThreads          = int.Parse(minThreads.Text);
                    opt.PeersFromTracker    = int.Parse(peersFromTrackers.Text);
                    opt.ConnectionTimeout   = int.Parse(conTimeout.Text);
                    opt.HandshakeTimeout    = int.Parse(handTimeout.Text);
                    opt.PieceTimeout        = int.Parse(pieceTimeout.Text);
                    opt.MetadataTimeout     = int.Parse(metaTimeout.Text);

                    opt.StatsCallback       = Stats;
                    opt.TorrentCallback     = TorrentInfo;
                    opt.StatusCallback      = StatusUpdate;

                    opt.EnableDHT           = true;

                    opt.Verbosity           = 0;
                    opt.LogDHT              = false;
                    opt.LogStats            = false;
                    opt.LogTracker          = false;
                    opt.LogPeer             = false;
                
                    output.Text     = "Started at " + DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo) + "\r\n";
                    button1.Text    = "Stop";

                    if ( File.Exists(input.Text.Trim()) ) 
                        tr = new TorSwarm(input.Text.Trim(), opt);
                    else
                        tr = new TorSwarm(new Uri(input.Text.Trim()), opt);
                    tr.Start();
                }
                catch (Exception e1)
                {
                    output.Text += e1.Message + "\r\n" + e1.StackTrace;
                    button1.Text = "Start";
                }

            } else
            {
                tr.Stop();
                button1.Text = "Start";
            }
        }

        private void TorrentInfo(Torrent torrent)
        {
            if ( InvokeRequired )
            {
                BeginInvoke(new Action(() => TorrentInfo(torrent)));
                return;
            } else
            {
                this.torrent = torrent;
                string str = "Name ->\t\t" + torrent.file.name + "\r\nSize ->\t\t" + Utils.BytesToReadableString(torrent.data.totalSize) + "\r\n\r\nFiles\r\n==============================\r\n";

                for (int i=0; i<torrent.data.files.Count; i++)
                    str += torrent.data.files[i].FileName + "\t\t(" + Utils.BytesToReadableString(torrent.data.files[i].Size) + ")\r\n";

                output.Text += str;
            }
        }
        private void StatusUpdate(int status, string errMsg)
        {
            if ( InvokeRequired )
            {
                BeginInvoke(new Action(() => StatusUpdate(status, errMsg)));
                return;
            } else
            {
                if ( status == 0 )
                {
                    if ( torrent.file.name != null ) MessageBox.Show("Downloaded successfully!\r\n" + torrent.file.name);
                    output.Text += "\r\n\r\nFinished at "   + DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo);
                } else
                {
                    output.Text += "\r\n\r\nStopped at "    + DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo);

                    if ( status == 2 ) 
                    {
                        MessageBox.Show("An error occured :( " + errMsg);
                        output.Text += "\r\n\r\n" + "An error occurred :(\r\n\t" + errMsg;
                    }
                }

                button1.Text = "Start";
            }
        }
        private void Stats(TorSwarm.StatsStructure stats)
        {
            if ( InvokeRequired )
            {
                BeginInvoke(new Action(() => Stats(stats)));
                return;
            } else
            {
                downRate.Text       = String.Format("{0:n0}", (stats.DownRate / 1024)) + " KB/s";
                downRateAvg.Text    = String.Format("{0:n0}", (stats.AvgRate  / 1024)) + " KB/s";
                maxRate.Text        = String.Format("{0:n0}", (stats.MaxRate  / 1024)) + " KB/s";
                eta.Text            = TimeSpan.FromSeconds((stats.ETA + stats.AvgETA)/2).ToString(@"hh\:mm\:ss");
                etaAvg.Text         = TimeSpan.FromSeconds(stats.AvgETA).ToString(@"hh\:mm\:ss");
                etaCur.Text         = TimeSpan.FromSeconds(stats.ETA).ToString(@"hh\:mm\:ss");

                bDownloaded.Text    = Utils.BytesToReadableString(stats.BytesDownloaded);
                bDropped.Text       = Utils.BytesToReadableString(stats.BytesDropped);
                pPeers.Text         = stats.PeersTotal.ToString();
                pInqueue.Text       = stats.PeersInQueue.ToString();
                pConnected.Text     = stats.PeersConnected.ToString();
                pFailed.Text        = (stats.PeersFailed1 + stats.PeersFailed2).ToString();
                pFailed1.Text       = stats.PeersFailed1.ToString();
                pFailed2.Text       = stats.PeersFailed2.ToString();
                pChoked.Text        = stats.PeersChoked.ToString();
                pUnchocked.Text     = stats.PeersUnChoked.ToString();
                pDownloading.Text   = stats.PeersDownloading.ToString();
                pDropped.Text       = stats.PeersDropped.ToString();

                if ( torrent != null && torrent.data.totalSize != 0) 
                    progress.Value  = (int) (stats.BytesDownloaded * 100.0 / torrent.data.totalSize);
            }

        }
        
        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            Cursor = Cursors.Default;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (filenames.Length > 0) input.Text = filenames[0];
            } else
            {
                input.Text = e.Data.GetData(DataFormats.Text, false).ToString();
            }
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( tr != null) tr.Stop();
        }
    }
}

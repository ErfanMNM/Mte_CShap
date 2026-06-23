package com.scansdk.sample;

import android.graphics.Color;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.TextView;

import com.android.decode.BarcodeManager;
import com.android.decode.DecodeResult;
import com.android.decode.ReadListener;
import com.android.decode.StartListener;
import com.android.decode.StopListener;
import com.android.decode.TimeoutListener;

public class MainActivity extends AppCompatActivity {
    static final String TAG = "MainActivity";

    private BarcodeManager mBarcodeManager;
    private ReadListener readListener;
    private StartListener startListener;
    private StopListener stopListener;
    private TimeoutListener timeoutListener;
    private TextView mTextview;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        mTextview = findViewById(R.id.textid);
    }

    private boolean initScan() {
        try {
            BarcodeManager localBarcodeManager = new BarcodeManager();
            localBarcodeManager.isInitialized();
            this.mBarcodeManager = localBarcodeManager;

            this.readListener = new ReadListener() {
                public void onRead(DecodeResult DecodeResult) {

                    Log.e(TAG, "onRead   BarcodeID"+ DecodeResult.getBarcodeID().toString());
                    Log.e(TAG, "BarcodeText"+ DecodeResult.getText());
                    mTextview.setText(DecodeResult.getText());
                }
            };
            this.startListener = new StartListener() {
                public void onScanStarted() {
                    Log.e(TAG, "onstart   BarcodeID");
                }
            };
            this.stopListener = new StopListener() {
                public void onScanStopped() {
                    Log.e(TAG, "onstop   BarcodeID");
                }
            };
            this.timeoutListener = new TimeoutListener() {
                public void onScanTimeout() {
                    Log.e(TAG, "onTimeout   BarcodeID");
                }
            };
            this.mBarcodeManager.addReadListener(this.readListener);
            this.mBarcodeManager.addStartListener(this.startListener);
            this.mBarcodeManager.addStopListener(this.stopListener);
            this.mBarcodeManager.addTimeoutListener(this.timeoutListener);
            return true;
        } catch (Exception localException) {
            return false;
        }
    }
    protected void onResume() {
        super.onResume();
        initScan();
    }
    protected void onPause() {
        super.onPause();
        try {
            if (this.mBarcodeManager != null) {
                this.mBarcodeManager.stopDecode();
                this.mBarcodeManager.removeReadListener(this.readListener);
                this.mBarcodeManager.removeStartListener(this.startListener);
                this.mBarcodeManager.removeStopListener(this.stopListener);
                this.mBarcodeManager.removeTimeoutListener(this.timeoutListener);
                this.mBarcodeManager.release();
                this.mBarcodeManager = null;
            }
            return;
        } catch (Exception localException) {

        }
    }

    protected void onDestroy() {
        super.onDestroy();
    }
}

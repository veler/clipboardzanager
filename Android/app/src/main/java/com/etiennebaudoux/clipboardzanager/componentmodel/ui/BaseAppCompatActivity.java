package com.etiennebaudoux.clipboardzanager.componentmodel.ui;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.support.v4.app.NavUtils;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;

import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.R;

/**
 * A basic activity.
 */
public abstract class BaseAppCompatActivity extends AppCompatActivity {
    private FrameLayout _viewStub;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        App.setCurrentActivity(this);

        super.setContentView(R.layout.activity_baseappcompat);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        _viewStub = (FrameLayout) findViewById(R.id.view_stub);


        setSupportActionBar(toolbar);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        getSupportActionBar().setDisplayShowHomeEnabled(true);
    }

    @Override
    public void onBackPressed() {
        NavUtils.navigateUpFromSameTask(this);
    }

    @Override
    protected void onPause() {
        App.setCurrentActivity(null);
        super.onPause();
    }

    @Override
    protected void onResume() {
        super.onResume();
        App.setCurrentActivity(this);
    }

    @Override
    public void setContentView(int layoutResID) {
        if (_viewStub != null) {
            LayoutInflater inflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
            ViewGroup.LayoutParams lp = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
            View stubView = inflater.inflate(layoutResID, _viewStub, false);
            _viewStub.addView(stubView, lp);
        }
    }

    @Override
    public void setContentView(View view, ViewGroup.LayoutParams params) {
        if (_viewStub != null) {
            _viewStub.addView(view, params);
        }
    }

    protected <T extends Activity> void navigateTo(Class<T> type) {
        Intent intent = new Intent(this, type);
        startActivity(intent);
    }
}

package com.etiennebaudoux.clipboardzanager.componentmodel.ui;

import android.app.Activity;
import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.support.design.widget.NavigationView;
import android.support.v4.widget.DrawerLayout;
import android.support.v7.app.ActionBarDrawerToggle;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.SubMenu;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;

import com.etiennebaudoux.clipboardzanager.App;
import com.etiennebaudoux.clipboardzanager.R;
import com.etiennebaudoux.clipboardzanager.activities.LinkAccountActivity;
import com.etiennebaudoux.clipboardzanager.componentmodel.services.CloudStorageService;
import com.etiennebaudoux.clipboardzanager.componentmodel.services.ServiceLocator;

/**
 * A basic activity with a hamburger menu.
 */
public abstract class BaseNavigationDrawerAppCompatActivity extends AppCompatActivity implements MenuItem.OnMenuItemClickListener {
    private FrameLayout _viewStub;
    private DrawerLayout _drawer;
    private ActionBarDrawerToggle _drawerToggle;
    NavigationView _navigationView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        App.setCurrentActivity(this);

        super.setContentView(R.layout.activity_basenavigationdrawerappcompat);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        _navigationView = (NavigationView) findViewById(R.id.navigation_view);
        _viewStub = (FrameLayout) findViewById(R.id.view_stub);
        _drawer = (DrawerLayout) findViewById(R.id.drawer);

        _drawerToggle = new ActionBarDrawerToggle(this, _drawer, toolbar, R.string.drawer_open, R.string.drawer_close);
        _drawer.addDrawerListener(_drawerToggle);

        setSupportActionBar(toolbar);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        View headerView = _navigationView.getHeaderView(0);
        headerView.setOnClickListener(onHeaderClick);

        if (ServiceLocator.getService(CloudStorageService.class).isLinkedToAService()) {
            headerView.findViewById(R.id.nav_header_connected).setVisibility(View.VISIBLE);
            headerView.findViewById(R.id.nav_header_not_connected).setVisibility(View.GONE);
        } else {
            headerView.findViewById(R.id.nav_header_connected).setVisibility(View.GONE);
            headerView.findViewById(R.id.nav_header_not_connected).setVisibility(View.VISIBLE);
        }

        Menu menu = _navigationView.getMenu();
        for (int i = 0; i < menu.size(); i++) {
            MenuItem menuItem = menu.getItem(i);
            menuItem.setOnMenuItemClickListener(this);
            if (menuItem.hasSubMenu()) {
                SubMenu subMenu = menuItem.getSubMenu();
                for (int j = 0; j < subMenu.size(); j++) {
                    subMenu.getItem(j).setOnMenuItemClickListener(this);
                }
            }
        }
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
        _drawerToggle.onConfigurationChanged(newConfig);
    }

    @Override
    protected void onPause() {
        App.setCurrentActivity(null);
        super.onPause();
    }

    @Override
    protected void onPostCreate(Bundle savedInstanceState) {
        super.onPostCreate(savedInstanceState);
        _drawerToggle.syncState();
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

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Pass the event to ActionBarDrawerToggle, if it returns
        // true, then it has handled the app icon touch event
        if (_drawerToggle.onOptionsItemSelected(item)) {
            return true;
        }        // Handle your other action bar items...

        return super.onOptionsItemSelected(item);
    }

    @Override
    public boolean onMenuItemClick(MenuItem item) {
        int id = item.getItemId();
        _navigationView.setCheckedItem(id);

        switch (id) {
            case R.id.home:
                _drawer.closeDrawers();
                // handle it
                break;
            case R.id.settings_general:
                _drawer.closeDrawers();
                // do whatever
                break;
            case R.id.settings_data:
                _drawer.closeDrawers();
                // do whatever
                break;
            case R.id.settings_security:
                _drawer.closeDrawers();
                // do whatever
                break;
            case R.id.settings_notifications:
                _drawer.closeDrawers();
                // do whatever
                break;
            case R.id.settings_about:
                _drawer.closeDrawers();
                // do whatever
                break;
            // and so on...
        }
        return false;
    }

    protected <T extends Activity> void navigateTo(Class<T> type) {
        Intent intent = new Intent(this, type);
        startActivity(intent);
    }

    private final View.OnClickListener onHeaderClick = new View.OnClickListener() {
        @Override
        public void onClick(View v) {
            if (ServiceLocator.getService(CloudStorageService.class).isLinkedToAService()) {
            } else {
                navigateTo(LinkAccountActivity.class);
            }
        }
    };
}

package com.etiennebaudoux.clipboardzanager.componentmodel.core;

import com.etiennebaudoux.clipboardzanager.componentmodel.core.tasks.Task;

import java.io.IOException;
import java.net.URL;
import java.util.concurrent.Callable;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;

/**
 * Provides a set of functions used to retrieve information about the operating system.
 */
public final class SystemInfoHelper {
    /**
     * Try to retrieve the title tag of an HTML page from a URI.
     *
     * @param uri The uri of the page
     * @return Returns the title of the page
     */
    public static Task<String> getWebPageTitle(String uri) {
        return new Task<>(new Callable<String>() {
            @Override
            public String call() throws Exception {
                try {
                    URL url = new URL(uri);
                    OkHttpClient client = new OkHttpClient();
                    Request request = new Request.Builder().url(url).build();

                    try (Response response = client.newCall(request).execute()) {
                        String html = response.body().string();
                        html = html.replaceAll("\\s+", " ");
                        Pattern p = Pattern.compile("<title>(.*?)</title>");
                        Matcher m = p.matcher(html);
                        if (m.find()) {
                            return m.group(1);
                        }
                    }
                } catch (IOException e) {
                    e.printStackTrace();
                }

                return "";
            }
        });
    }
}

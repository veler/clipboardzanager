package com.etiennebaudoux.clipboardzanager.enums;

import android.support.annotation.IntDef;

import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;

@IntDef({ThumbnailDataType.UNKNOWN, ThumbnailDataType.STRING, ThumbnailDataType.FILE,
        ThumbnailDataType.BITMAP, ThumbnailDataType.LINK, ThumbnailDataType.COLOR})
@Retention(RetentionPolicy.SOURCE)
public @interface ThumbnailDataType {
    int UNKNOWN = 0;
    int STRING = 1;
    int FILE = 2;
    int BITMAP = 3;
    int LINK = 4;
    int COLOR = 5;
}

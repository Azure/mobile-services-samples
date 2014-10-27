package com.example.CallCustomApi;

import com.google.gson.annotations.SerializedName;

public class MarkAllResult {
    @SerializedName("count")
    public int mCount;

    public int getCount() {
        return mCount;
    }

    public void setCount(int count) {
            this.mCount = count;
    }
}

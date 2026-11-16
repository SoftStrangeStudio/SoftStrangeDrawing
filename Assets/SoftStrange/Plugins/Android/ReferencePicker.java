package com.softstrange.drawing;
import android.app.Activity;
import android.app.Fragment;
import android.content.Intent;
import android.os.Bundle;
import android.net.Uri;
import com.unity3d.player.UnityPlayer;
import java.io.*;

public final class ReferencePicker extends Fragment {
 public static void pick(Activity activity){activity.runOnUiThread(()->{try{ReferencePicker picker=new ReferencePicker();activity.getFragmentManager().beginTransaction().add(picker,"softstrange-reference-picker").commitAllowingStateLoss();}catch(Exception e){report("ERROR:Could not open file picker: "+e.getMessage());}});}
 @Override public void onCreate(Bundle state){super.onCreate(state);Intent intent=new Intent(Intent.ACTION_OPEN_DOCUMENT);intent.setType("image/*");intent.addCategory(Intent.CATEGORY_OPENABLE);intent.putExtra(Intent.EXTRA_MIME_TYPES,new String[]{"image/png","image/jpeg"});try{startActivityForResult(intent,91);}catch(Exception e){report("ERROR:No file picker available. Your bundled outlines still work; copy an image into Inbox over USB.");getFragmentManager().beginTransaction().remove(this).commitAllowingStateLoss();}}
 @Override public void onActivityResult(int request,int result,Intent data){super.onActivityResult(request,result,data);if(request==91&&result==Activity.RESULT_OK&&data!=null){try{Uri uri=data.getData();File file=new File(getActivity().getCacheDir(),"reference-"+System.nanoTime()+".image");try(InputStream input=getActivity().getContentResolver().openInputStream(uri);OutputStream output=new FileOutputStream(file)){byte[] buffer=new byte[8192];int count,total=0;while((count=input.read(buffer))!=-1){total+=count;if(total>12582912)throw new IOException("Image must be under 12 MB");output.write(buffer,0,count);}}report(file.getAbsolutePath());}catch(Exception e){report("ERROR:"+e.getMessage());}}getFragmentManager().beginTransaction().remove(this).commitAllowingStateLoss();}
 static void report(String value){UnityPlayer.UnitySendMessage("Soft Strange Drawing","ImportFile",value);}
}

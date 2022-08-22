using System;
using System.Collections;
using System.Collections.Generic;

public static class EnsureNotNull
{
  public static void Objects(params Object[] objects)
  {
    foreach (Object thing in objects) if (thing == null)
      {
        // Get unity's helpful error mesage in ours
        try
        {
          thing.GetType();
        }
        catch (System.Exception error)
        {
          throw new System.Exception(
            "Got a null reference to some component. Unity's error message:\n" + error.Message
          );
        }
      }
  }
}
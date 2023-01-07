# Compare view

In the **Compare** view you can load two data sets, which the Profiler Analyzer displays in two different colors. It displays the information from both data sets in a similar way to the [Single view](single-view.md) and has the same panes and panels.

For information on navigating the window, see the [Profile Analyzer window navigation](profile-analyzer-window.html/#window-navigation) documentation.

![Compare View](images/profile-analyzer-compare-view.png)<br/>*The Compare view with two data sets loaded.*

## Loading data

To load data into the **Compare** view, select the **Pull Data** button in the frame control pane, and the Profile Analyzer pulls in any data in the [Profiler](https://docs.unity3d.com/Manual/Profiler.html) window. Alternatively, select the **Load** button to load Profile Analyzer (.pdata) data you have saved from a previous session.

>[!NOTE]
>If you select the **Load** option, the data must be in the Profile Analyzer .pdata format. If you have data from the Profiler in the .data file format, open it in the Profiler first, and then select the **Pull Data** button in the Profile Analyzer.

For more information on how to pull data into the Profile Analyzer, see the workflow documentation on [Collecting and viewing data](collecting-and-viewing-data.md).

## Marker Comparison list

The **Marker Comparison** pane contains a sortable list of markers with a number of useful statistics, including the difference between the two sets. The proportional graphs with the `<` and `>` labels visualize the values of each marker, so you can see the difference between the timings of both samples.  

If you select a marker in the list, the **Marker Summary** panel displays in depth information on the marker. Each marker in the list is an aggregation of all the instances of that marker, across all filtered threads and in all ranged frames.

You can filter the columns in the **Marker Comparison** list to a more relevant set. This is particularly useful if you want to filter out irrelevant data when you look for **Time** or **Count** values. To filter the columns, select the **Marker columns** dropdown from the **Filters** pane. For more information on how to filter data, see the [Filters](filtering-system.md) documentation.

### Marker Comparison columns and groups

By default, the **Marker columns** dropdown in the **Filters** pane has six preset column layouts that you can use to adjust the layout of the **Marker Comparison** pane. They are:

* **Time and count:** Displays information on the average timings and number of times the markers were called.
* **Time:** Displays information on the average timings of the markers.
* **Totals:** Displays information about the total amount of time the markers took on the whole data set.
* **Time with totals:** Displays information about both the average and total times of the markers.
* **Count totals:** Displays information about the total number of times the markers were called.
* **Count per frame:** Displays information on the average total per frame the markers were called.
* **Depths:** Displays information on where the markers are in the Hierarchy. For more information, see the documentation on Depth Slices in [Filters pane](filtering-system.html#depth-slice).
* **Threads:** Displays the name of the thread that the markers appear on. For more information, see the documentation on the Thread window in [Filters pane](filtering-system.html#thread-window).

You can also use the **Custom** column layout to select your own custom mix of columns to add to the layout. To do this, right click on the header of any column, and manually enable or disable any of the columns as necessary.

![Context menu](images/marker-details-compare-custom-columns.png)<br/>*The list of columns you can add to the Marker Comparison pane*

>[!NOTE]
>In this pane, the **Left** label refers to the first data set loaded into the **Frame Control** pane, which is colored blue. The **Right** label refers to the second data set, which is colored orange.

The following table shows the columns that the Profile Analyzer displays when you select that layout.

||**Time and count**| **Time**| **Totals**| **Time with totals**| **Count totals**| **Count per frame** |**Depths**|**Threads**|
|---|---|---|---|---|---|---|---|---|
|**Marker Name**|&#10003;|&#10003;|&#10003;|&#10003;|&#10003;|&#10003;|&#10003;|&#10003;|
|**Left Median**<br/> **Right Median**|&#10003;|&#10003;||&#10003;|||||
|`<`<br/> `>`|&#10003;|&#10003;||&#10003;|||||
|**Diff**|&#10003;|&#10003;|||||||
|**Diff Percent**|||||||||
|**Abs Diff**|&#10003;|&#10003;||&#10003;|||||
|**Count Left** <br/> **Count Right**|&#10003;||||&#10003;||||
|`<` **Count** <br/> `>` **Count**|||||&#10003;||||
|**Count Delta**|&#10003;||||&#10003;||||
|**Count Delta Percent**|||||||||
|**Abs Count**|||||&#10003;||||
|**Count Left Frame** <br/> **Count Right Frame**||||||&#10003;|||
|`<` **Frame Count** <br/> `>` **Frame Count**||||||&#10003;|||
|**Count Delta Frame**||||||&#10003;|||
|**Count Delta Percent Frame**|||||||||
|**Abs Frame Count**||||||&#10003;|||
|**Total Left** <br/> **Total Right**|||&#10003;|&#10003;|||||
|`<` **Total** <br/> `>` **Total**|||&#10003;|&#10003;|||||
|**Total Delta**|||&#10003;||||||
|**Total Delta Percent**|||||||||
|**Abs Total**|||&#10003;|&#10003;|||||
|**Depth Left** <br/> **Depth Right**|||||||&#10003;||
|**Depth Diff**|||||||&#10003;||
|**Threads Left** <br/> **Threads Right**||||||||&#10003;|

The following table explains what each column does:

|**Column**|**Description**|
|---|---|
|**Marker Name**| Displays the name of the marker.|
|**Left Median** <br/> **Right Median** | The sum of activity for the marker. **Left Median** displays the first data set loaded into the **Frame Control** pane, colored blue. **Right Median** displays the second data set loaded into the **Frame Control** pane, colored orange.|
|`<` <br/> `>`|A visual representation of the **Left Median** (`<`) and **Right Median** (`>`) data.|
|**Diff**|The difference between the summed values in each data set. Negative values mean that the left (blue) set of data is bigger, positive means the right (orange) set of data is bigger.|
|**Diff Percent**|The difference relative to the first data set.|
|**Abs Diff**|The absolute difference between the summed values in each data set.|
|**Count Left**<br/>**Count Right**|The number of times the marker started or stopped. **Count Left** displays the first data set loaded into the **Frame Control** pane, colored blue. **Count Right** displays the second data set loaded into the **Frame Control** pane, colored orange.|
|`<` **Count** <br/> `>` **Count**|A visual representation of the **Count Left** and **Count Right** data.|
|**Count Delta**|The difference between the **Count** values in each data set. Negative values mean that the left (blue) set of data is bigger, positive means the right (orange) set of data is bigger.|
|**Count Delta Percent**|The difference in count relative to the first data set.|
|**Abs Count**|The absolute difference between the **Count** values for the selected frames. Negative values mean that the left (blue) set of data is bigger, positive means the right (orange) set of data is bigger.|
|**Count Left Frame** <br/> **Count Right Frame**| The average count of the marker over all non-zero frames. **Count Left Frame** displays the first data set loaded into the **Frame Control** pane, colored blue. **Count Right Frame** displays the second data set loaded into the **Frame Control** pane, colored orange.|
|`<` **Frame Count** <br/> `>` **Frame Count**|A visual representation of the **Count Left Frame** and **Count Right Frame** data.|
|**Count Delta Frame**|The difference between the **Count Left Frame** and **Count Right Frame** values. Negative values mean that the left (blue) set of data is bigger, positive means the right (orange) set of data is bigger.|
|**Count Delta Percent Frame**|The difference in average count relative to the first data set.|
|**Abs Frame Count**|The absolute difference between the number of times the marker started or stopped in each data set.|
|**Total Left** <br/> **Total Right**|The total time for the marker over the selected frames. **Total Left** displays the first data set loaded into the **Frame Control** pane, colored blue. **Total Right** displays the second data set loaded into the **Frame Control** pane, colored orange.|
|`<` **Total** <br/> `>` **Total**|A visual representation of the **Total Left** and **Total Right** data.|
|**Total Delta**|The difference between the total times over the selected frames in each data set. Negative values mean that the left (blue) set of data is bigger, positive means the right (orange) set of data is bigger.|
|**Total Delta Percent**|The difference in total time relative to the first data set.|
|**Abs Total**|The absolute difference between the total times over all of the selected frames in each data set.|
|**Depth Left** <br/> **Depth Right**|The level, or depth, that the marker appears at. The marker might appear on multiple depth levels. **Depth Left** displays the first data set loaded into the **Frame Control** pane, colored blue. **Depth Right** displays the second data set loaded into the **Frame Control** pane, colored orange.|
|**Depth Diff**|The difference between the **Depth Left** and **Depth Right** values.|
|**Threads Left** <br/> **Threads Right**|The name of the thread that the marker appears on. **Threads Left** displays the first data set loaded into the **Frame Control** pane, colored blue. **Threads Right** displays the second data set loaded into the **Frame Control** pane, colored orange.|

### Marker Comparison context menu commands

If you right-click on a marker in the **Marker Comparison** list you can control the filter and list even further.

![Context menu](images/marker-details-context-menu.png)<br/>*The context menu of the Marker Comparison pane*

|**Command**|**Function**|
|---|---|
|**Select Frames that contain this marker (within whole data set)**| Select all the frames from the entire data set that contain an instance of this marker.|
|**Select Frames that contain this marker (within current selection)**| Select all the frames from a selected range of data that contain an instance of this marker.|
|**Select All**| Selects the entire data set, if you have a range of data selected.|
|**Add to / Remove From Include Filter**| Add or remove the selected marker to the **Include** filter. This filters the marker list to only markers that match.|
|**Add to Exclude Filter**| Add the selected marker to the **Exclude** filter. This removes the marker from the marker list. This is useful if you want to remove markers that are using up resources and skewing the markers that you are interested in.|
|**Set as Parent Marker Filter**| Limit the analysis to this marker and markers included below it on the callstack. For more information, see the [Parent Marker](filtering-system.html#parent-marker) documentation on the Filters page.|
|**Clear Parent Marker Filter**| Select this to clear the marker as a parent marker filter.|
|**Copy To Clipboard**| Copies the selected value to the clipboard.|

## Analyzing data in Compare view

For further information on how to analyze data in Compare view, see the workflow documentation on [Comparing frames from different data sets](comparing-frames-same-dataset.md) and [Comparing frames from the same data set](comparing-frames-same-dataset.md).

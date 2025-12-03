using System.Data;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class GUIManager : MonoBehaviour
{

    UIDocument UIDoc;
    Button recordButton;
    Label imageCounterLabel;
    Image mainDisplay;

    MainExporter exporter;
    MainRandomizer randomizer;

    private void Awake()
    {
        exporter = GameObject.FindGameObjectWithTag("Exporter").GetComponent<MainExporter>();
        randomizer = GameObject.FindGameObjectWithTag("Generator").GetComponent<MainRandomizer>();

        UIDoc = GetComponent<UIDocument>();
        if (!UIDoc)
        {
            Debug.LogWarning("UIDocument not found in the GUI while linking buttons");
            return;
        }
        UIDoc.panelSettings.clearColor = true;


        mainDisplay = new Image();
        mainDisplay.AddToClassList("mainImageDisplay");
        UIDoc.rootVisualElement.Q<VisualElement>("RenderDisplay").Add(mainDisplay);
    }

    public void Start()
    {
        exporter.endOfDatasetGeneration += updateFileCounter;


        recordButton = UIDoc.rootVisualElement.Q<Button>("RecordButton");
        recordButton.RegisterCallback<ClickEvent>(ev => recordButtonClicked());
        updateRecordButton();
        exporter.endOfDatasetGeneration += updateRecordButton;

        imageCounterLabel = UIDoc.rootVisualElement.Q<Label>("ImageCounter");
        updateFileCounter();
        exporter.renderEnd += updateFileCounter;

        UIDoc.rootVisualElement.Q<Button>("RandomizeAll").RegisterCallback<ClickEvent>(ev => randomizer.Randomize());

        linkTextures();
    }

    private void linkTextures()
    {
        var PreviewList = UIDoc.rootVisualElement.Q<ScrollView>("PreviewList");

        foreach (var pass in exporter.exporters)
        {
            foreach(var titleTexturePair in pass.getTextureOutputs())
            {
                var image = new Image();
                image.scaleMode = ScaleMode.ScaleToFit;
                image.image = titleTexturePair.Item2;
                if(mainDisplay.image == null)
                    mainDisplay.image = titleTexturePair.Item2;
                image.AddToClassList("previewImage");
                float aspectRatio = (float)titleTexturePair.Item2.height / (float)titleTexturePair.Item2.width;
                image.RegisterCallback<GeometryChangedEvent>(evt =>
                    image.style.height = image.resolvedStyle.width * aspectRatio);

                var button = new Button();
                button.text = titleTexturePair.Item1;
                button.AddToClassList("previewButton");
                button.RegisterCallback<ClickEvent>(ev => mainDisplay.image = titleTexturePair.Item2);

                var outputPreview = new VisualElement();
                outputPreview.AddToClassList("previewListElement");
                outputPreview.Add(image);
                outputPreview.Add(button);
                PreviewList.Add(outputPreview);
            }
        }

    }

    private void OnDestroy()
    {
        UIDoc.panelSettings.clearColor = false;
    }

    public void recordButtonClicked()
    {
        exporter.capturing = !exporter.capturing;
        updateRecordButton();
    }

    public void updateRecordButton()
    {
        if (recordButton == null)
            return;
        recordButton.text = exporter.capturing ? "Stop recording" : "Start recording";
        recordButton.AddToClassList(exporter.capturing ? "RecordButton_Recording" : "RecordButton_NotRecording");
        recordButton.RemoveFromClassList(!exporter.capturing ? "RecordButton_Recording" : "RecordButton_NotRecording");
    }

    public void updateFileCounter()
    {
        if (imageCounterLabel == null)
            return;
        imageCounterLabel.text = $"Counter:\n{exporter.fileCounter}";
    }
}

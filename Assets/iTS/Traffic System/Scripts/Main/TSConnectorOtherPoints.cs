using System;

[Serializable]
public class TSConnectorOtherPoints : TSOtherPoints
{
    [NonSerialized] private TSConnectorPoint _point;
    [NonSerialized] private TSLaneConnector _connector;
    [NonSerialized] private TSLaneInfo _lane;
    public TSConnectorPoint Point => _point;
    public TSLaneConnector Connector => _connector;
    public TSLaneInfo Lane => _lane;

    public void SetPointReference(TSLaneInfo lane, TSLaneConnector connector, TSConnectorPoint point)
    {
        _point = point;
        _lane = lane;
        _connector = connector;
    }
}
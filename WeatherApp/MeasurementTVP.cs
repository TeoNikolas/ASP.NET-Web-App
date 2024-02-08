using System.Collections.Generic;

public class TimePeriod
{
    public string beginPosition { get; set; }
    public string endPosition { get; set; }
}

public class PhenomenonTime
{
    public TimePeriod TimePeriod { get; set; }
}

public class TimeInstant
{
    public string timePosition { get; set; }
}

public class NamedValue
{
    public string name { get; set; }
    public TimeInstant value { get; set; }
}

public class Parameter
{
    public NamedValue NamedValue { get; set; }
}

public class Location
{
    public int identifier { get; set; }
    public List<object> name { get; set; }
    public string representativePoint { get; set; }
    public string country { get; set; }
    public string timezone { get; set; }
    public string region { get; set; }
}

public class LocationCollection
{
    public Location Location { get; set; }
}

public class SampledFeature
{
    public LocationCollection LocationCollection { get; set; }
}

public class Point
{
    public string name { get; set; }
    public string pos { get; set; }
}

public class PointMembers
{
    public Point Point { get; set; }
}

public class MultiPoint
{
    public PointMembers pointMembers { get; set; }
}

public class SFSpatialSamplingFeature
{
    public SampledFeature sampledFeature { get; set; }
    public MultiPoint shape { get; set; }
}

public class FeatureOfInterest
{
    public SFSpatialSamplingFeature SF_SpatialSamplingFeature { get; set; }
}

public class MeasurementTVP
{
    public string time { get; set; }
    public double value { get; set; }
}

public class MeasurementTimeseries
{
    public List<MeasurementTVP> point { get; set; }
}

public class Result
{
    public MeasurementTimeseries MeasurementTimeseries { get; set; }
}

public class PointTimeSeriesObservation
{
    public PhenomenonTime phenomenonTime { get; set; }
    public TimeInstant resultTime { get; set; }
    public string procedure { get; set; }
    public Parameter parameter { get; set; }
    public string observedProperty { get; set; }
    public FeatureOfInterest featureOfInterest { get; set; }
    public Result result { get; set; }
}

public class Member
{
    public PointTimeSeriesObservation PointTimeSeriesObservation { get; set; }
}

public class FeatureCollection
{
    public List<Member> member { get; set; }
}

public class Root
{
    public FeatureCollection FeatureCollection { get; set; }
}

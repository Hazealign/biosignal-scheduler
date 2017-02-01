# Biosignal Scheduler

NSQ에서 받아온 생체 신호 정보를 주기적으로 DB와 디스크에 저장하는 스케쥴러 프로젝트입니다.

**Author**: [Haze Lee](http://github.com/Hazealign) / **Last Modify**: Fabruary 1, 2017

**ChangeLog**

- **v1.0**: 처음 설계로 구현된 내용을 기반으로 작성되었습니다. [rev. 7efda95]

## Json Schema

NSQ에 저장하는 Device Interface와 Scheduler는 암묵적으로 이 JSON 규격을 지키도록 설계되었습니다. 이를 통해 추후에 기기가 많아지더라도 현재의 JSON 전송 규격을 통일하면 쉽게 확장할 수 있습니다.

```
{
    "DEVICE": String,				// 어떤 Device Interface, Version에서 왔는지 정보
    "TIMESTAMP": Timestamp,			// 센서의 시간
    "TYPE": String,					// Observation Type
    "PORT": String,					// 디바이스의 포트 번호
    "HOST": String,					// 수집한 서버의 IP
    "UDID": String,					// 기기의 고유 Device ID
    "PATIENT_ID": String,			// 기기에서 받아온 환자 ID
    "VALUE_UNIT": String,			// 값의 단위
    TYPE: [Double or List<Double>]
  	// Key는 TYPE의 값이며, Value는 Waveform이냐 Numeric이냐에 따라 타입이 달라짐.
}
```

## How Works?

프로그램이 실행되면 `Scheduler`가 동작하기 시작합니다. `Scheduler`에서는 NSQ와 연결하기 위한 `ConsumerManager` 인스턴스를 생성하게 되며, 10분 간격으로 동작하는(`Observable.interval`을 이용함.) 스케쥴러를 내부에 만듭니다. 매 10분마다 `ConsumerManager`에 차곡차곡 쌓인 센서 데이터들을 각 오퍼레이터(`IScheduleOperator` 인터페이스를 상속해야함.)의 `Operate(List<MqModel>)` 메소드에 실어서 호출합니다.

각 오퍼레이터(`NumericExport`, `WaveformExport`)는 데이터베이스와 연결하기 위한 `SqlHelper` 인스턴스를 가지고 있습니다. `MqModel`의 `IsNumeric` 플래그를 통해 Numeric이냐 Waveform이냐를 분류하게 되며, Waveform에서는 CSV로 Export할 수 있는 기능이 추가로 구현되었습니다.

`SqlHelper`는 맵핑 테이블(Numeric - Observation Type, PatientId - AnonymousId) 등을 가져와서 저장하고 관리하는 기능이 들어있으며, Numeric 혹은 Waveform에 맞는 `MqModel`을 SQL Server에 저장할 수 있는 기능도 가지고 있습니다.

## References

편의상 싱글턴 클래스는 `_instance`와 생성자를 스킵합니다. 대신 팩토리 메소드인 `Instance`에 대한 설명은 남겨둡니다.

### BiosignalScheduler.Export

### NumericExport: IScheduleOperator

#### Properties

- **[READONLY]** SqlHelper **_helper**
  - `SqlHelper`의 싱글톤 인스턴스. DB에 데이터를 쌓기 위해 쓴다.

#### Factory Methods

- List<MqModel> **Filter**(IEnumerable<MqModel> origin)
  - `MqModel.IsNumeric`이 참인 값만 분류해서 리스트로 반환한다.

#### Instance Methods

- void **Operate**(List<MqModel> data)
  - `Filter`를 거쳐서 각각 하나하나 `_helper`에 쌓아준다.

### WaveformExport: IScheduleOperator

#### Inner Class

- **TypeComparer**: EqualityComparer<MqModel>
  - `MqModel.PatientId`와 `MqModel.Type` 값이 같으면 같은 값으로 간주한다.
  - `null` 값에 대한 예외처리가 되어있다.

#### Properties

- **[READONLY]** SqlHelper **_helper**
  - `SqlHelper`의 싱글톤 인스턴스. DB에 데이터를 쌓기 위해 쓴다.

#### Factory Methods

- List<MqModel> **Filter**(IEnumerable<MqModel> origin)
  - `MqModel.IsNumeric`이 거짓인 값만 분류해서, `TypeComparer`를 통해 중복된 값을 제외하고 리스트로 반환한다.
- *private* string **WriteToCsv**(string patientId, string type, List<MqModel> data, DateTime startDate, DateTime endDate)
  1. `data`에서 `Type`가 메소드의 인자 `type`이랑 같고, `PatientId`가 인자 `patientId`와 같은 값을 찾아서 시간 순서대로 정렬한다.
  2. 새로운 `List<double>`를 만들고 1의 값을 새 리스트에 담는다.
  3. `startDate`와 `endDate`를 바탕으로 파일이 저장될 `filePath`를 선언한다.
  4. 서브 디렉토리들이 없으면 폴더를 생성하고, csv 파일을 작성한다. (이 파일은 60줄이 되도록 계산되어있다.)
  5. **저장한 `filePath`를 반환한다.**

#### Instance Methods

- void **Operate**(List<MqModel> data)
  - `Filter`를 거쳐서 각각 하나하나 `WriteToCsv`에 담고, 받아온 `filePath`를 바탕으로 `_helper`를 통해 SQL에 저장한다.

### BiosignalScheduler.Model

### DatabaseModel

#### Inner Class

- *[Serializable]* **MappingTable**
  - string **Observation**, string **WaveName**, long **ObservationType**로 구성된 모델 클래스.
- *[Serializable]* **PatientIdMap**
  - string **PatientId**, string **AnonymousId**로 구성된 모델 클래스.

### MqModel

#### Properties

- **[Lazy]** bool **IsNumeric**: `GetValue()`로 받은 Type이 `List<double>`이 아니면 참을 반환한다.
- string **Timestamp**
  - Not Nullable, JsonProperty *TIMESTAMP*
- string **Type**
  - Not Nullable, JsonProperty *TYPE*
- string **Port**
  - Not Nullable, JsonProperty *PORT*
- string **Host**
  - Not Nullable, JsonProperty *HOST*
- string **Unit**
  - Not Nullable, JsonProperty *UNIT*
- string **UniqueDeviceId**
  - Not Nullable, JsonProperty *UDID*
- string **PatientId**
  - Not Nullable, JsonProperty *PATIENT_ID*
- double **BloodPressureSys**
  - Nullable, JsonProperty *BLOOD_PRESSURE_SYS*
- double **BloodPressureDia**
  - Nullable, JsonProperty *BLOOD_PRESSURE_DIA*
- double **BloodPressureMean**
  - Nullable, JsonProperty *BLOOD_PRESSURE_MEAN*
- double **EtCo2**
  - Nullable, JsonProperty *ET_CO2*
- double **AirwayRespRate**
  - Nullable, JsonProperty *AIRWAY_RESP_RATE*
- List<double> **PlethWave**
  - Nullable, JsonProperty *PLETH_WAVE*
- double **HeartRate**
  - Nullable, JsonProperty *HEART_RATE*
- double **SpO2**
  - Nullable, JsonProperty *SPO2*
- List<double> **EcgWave**
  - Nullable, JsonProperty *ECG_WAVE*

#### Instance Methods

- object **GetValue**()
  - 반환될 수 있는 타입은 `double` 혹은 `List<double>`이다.
  - `Type`에 따라서 값을 보낸다.
- KeyValuePair<string, object> **GetKeyValue**()
  - 반환될 수 있는 타입은 정확하게는 `KeyValuePair<string, double>` 혹은 `KeyValuePair<string, List<double>>`이다.
  - Type을 같이 보내주는게 특징이다.

### SqlHelper

#### Inner Class

- *[Serializable]* **Connection**
  - string **Server**, string **UserId**, string **Password**, string **Database**로 된 모델 클래스
  - `ToString()`을 통해 SQL Server 커넥션 파라미터를 생성한다.

#### Properties

- *private* Connection **_innerConn**
  - DB와 연결할 때를 위한 커넥션 정보, 생성자부터 받아서 저장한다.
- **[READONLY]** *private* ConsoleLogger **_logger**
- **[READONLY]** *private* List<DatabaseModel.MappingTable> **_mappingTables**
  - Metric ID - Observation Type간의 맵핑 테이블.
- **[READONLY]** *private* List<DatabaseModel.PatientIdMap> **_patientIdMaps**
  - Patient ID - Anonymous ID간의 맵핑 테이블.

#### Factory Methods

- SqlHelper **Instance**
  - SqlHelper의 싱글턴 인스턴스를 반환합니다.
  - 객체가 없을 때 초기화되며, 생성자에선 `GetMappingTableAsync()`와 `GetPatientIdMapsAsync()`가 호출됩니다.
- *private* T **Await<T>**(Task<T> task)
  - 비동기 태스크를 블록킹 방식으로 실행한 뒤 결과를 반환합니다.
- *private* string **EncryptSha256**(string origin)
  - `origin`을 SHA-256 방식으로 암호화해서 반환합니다. 인코딩은 UTF-8을 전제로 합니다.
- string **DateTimeToString**(DateTime dt, string format = "{0:yyyyMMddHHmmss}")
  - `DateTime`을 `string`으로 포맷합니다.

#### Instance Methods

- string **GetAnonymousId**(string patientId)
  - `patientId`를 바탕으로 익명화된 ID를 가져옵니다.
- string **GetMetricId**(string origin)
  - `_mappingTables`에서 `origin`에 맞는 Metric ID를 가져옵니다.
- *private async* Task **AddPatientIdMap**(string patientId, string anonymousId)
  - Patient ID - Anonymous ID간의 새로운 맵핑 규격을 등록합니다.
  - SQL(Insert문)을 먼저 실행한 뒤, `_patientIdMap`에도 규칙이 추가됩니다.
- *private async* Task **InsertWaveformValueAsync**
  (MqModel model, string filePath, DateTime startTime, DateTime endTime)
  - **`model`이 Numeric이면 ArgumentException을 냅니다.**
  - DB에 데이터를 저장합니다.
- *private async* Task **InsertNumericValueAsync**(MqModel model)
  - **`model`이 Waveform이면 ArgumentException을 냅니다.**
  - DB에 데이터를 저장합니다.
- *async* Task<List<DatabaseModel.PatientIdMap>> **GetPatientIdMapsAsync**()
  - Patient ID - Anonymous ID간의 맵핑 규격을 DB에서 읽어옵니다.
- *async* Task<List<DatabaseModel.MappingTable>> **GetMappingTablesAsync**()
  - Metric ID - Observation Type간의 맵핑 규격을 DB에서 읽어옵니다.
- *async* Task **ExecuteAsync**(string sql)
  - DB에 특정한 SQL 구문을 실행합니다.

### BiosignalScheduler.Scheduler

### ConsumerManager

#### Inner Class

- **MessageHandler**: IHandler
  - 에러가 났을 때, 혹은 NSQ에서 메세지를 받았을 때의 콜백 핸들러.

#### Properties

- List<MqModel> **ConsumingList** *(set 불가능)*
  - NSQ에서 데이터를 수신하면 임시로 저장하는 메모리 내 저장 공간.
- *private* **[READONLY]** Consumer **_consumer**
  - NSQ와의 연결을 위한 Consumer 객체
  - 생성자에서 초기화된다.

#### Instance Methods

- void **Connect**()
  - NSQ에 연결한다.
- void **AddHandler<T>**(T handler) *where T: IHandler*
  - `_consumer`에 콜백 핸들러를 등록한다.

#### Factory Methods

- ConsumerManager **Instance**
  - ConsumerManager의 싱글턴 인스턴스를 반환합니다.

### IScheduleOperator

#### Interface Methods

- void **Operate**(List<MqModel> data)
  - `ConsumerManager`에서 Rx Loop를 돌 때 실행되는 각 오퍼레이터의 콜백 메소드

### Scheduler

#### Properties

- IDisposable **_loop**
  - 10분마다 동작하는 Rx Loop.
- **[READONLY]** *private* ConsoleLogger **_logger**
- *private* List<IScheduleOperator> **Operators** *(set 불가능)*
  - `_loop`에서 10분마다 호출하는 오퍼레이터 목록

#### Instance Methods

- **Constructer**()
  - 여기서 `ConsumerManager.Instance`를 NSQ에 연결합니다.
- void **Start**()
  - `_loop`의 루프를 시작합니다.
- void **Stop**()
  - `_loop`의 루프를 멈춥니다.
- void **AddOperator<T>**(T oper) *where T: IScheduleOperator*
  - `Operators`에 오퍼레이터를 추가합니다.

## Read Also

- [Reactive Extensions for .NET](https://msdn.microsoft.com/en-us/library/hh242985(v=vs.103).aspx)
- [NsqSharp](https://github.com/judwhite/NsqSharp)

#include <ArduinoJson.h>
#include <Stepper.h>
#include <ezButton.h>

DynamicJsonDocument doc(1024);
Stepper RotationalStepper = Stepper(2038, 4, 6, 5, 7);
const int stepPin = 10;
const int dirPin = 11;
const int TRIGGERPIN = 12;
ezButton limitSwitchMax(8);
ezButton limitSwitchMin(9);

void setup() {
  pinMode(stepPin, OUTPUT);
  pinMode(dirPin, OUTPUT);
  pinMode(TRIGGERPIN, OUTPUT);
  pinMode(LED_BUILTIN, OUTPUT);

  digitalWrite(stepPin, LOW);
  RotationalStepper.setSpeed(12);
  limitSwitchMax.setDebounceTime(50);
  limitSwitchMin.setDebounceTime(50);
  Serial.begin(9600);
}

void loop() {
  if (Serial.available())
  {
    String data = Serial.readString();
    char* input = data.c_str();

    deserializeJson(doc, input);
    String type = doc["Type"].as<String>();
    int value = doc["Value"]; 

    if(type == "r")
    {
      rotate_engine(value);
    }

    else if (type == "p")
    {
      move_engine(value);
    }

    else if (type == "t")
    {
      trigger(value);
    }

    else if (type == "c")
    {
      configuration_test(value);
    }
  } 
}

void rotate_engine(int value)
{
	RotationalStepper.step(value);
  Serial.println("Done");
}

void move_engine(int value) 
{
  if (value > 0)
  {
    digitalWrite(dirPin, HIGH);
  }
  else
  {
    digitalWrite(dirPin, LOW);
    value = -value;
  }

  for (int i = 0; i < value; i++) {
    limitSwitchMax.loop();
    limitSwitchMin.loop();
    
    if (limitSwitchMax.getState() == LOW)
    {
      delayMicroseconds(100000);
      digitalWrite(dirPin, LOW);

      for (int i = 0; i < 600; i++) {
          limitSwitchMax.loop();
          digitalWrite(stepPin, HIGH);
          delayMicroseconds(500);
          digitalWrite(stepPin, LOW);
          delayMicroseconds(500);
      }
      Serial.println("LimitPlus");
      return;
    }

    else if (limitSwitchMin.getState() == LOW)
    {
      delayMicroseconds(100000);
      digitalWrite(dirPin, HIGH);

      for (int i = 0; i < 600; i++) {
          limitSwitchMin.loop();
          digitalWrite(stepPin, HIGH);
          delayMicroseconds(500);
          digitalWrite(stepPin, LOW);
          delayMicroseconds(500);
      }
      Serial.println("LimitMinus");
      return;
    }
    digitalWrite(stepPin, HIGH);
    delayMicroseconds(500);
    digitalWrite(stepPin, LOW);
    delayMicroseconds(500);
  }
  Serial.println("Done");
}

void trigger(int value) 
{
  digitalWrite(TRIGGERPIN, HIGH);
  delay(value);
  digitalWrite(TRIGGERPIN, LOW);
  Serial.println("Done");
}

void configuration_test(int value) 
{
  digitalWrite(LED_BUILTIN, HIGH);
  delay(value);                      
  digitalWrite(LED_BUILTIN, LOW);   
  Serial.println("Done");
}
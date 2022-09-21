from re import L
import azure.cognitiveservices.speech as speechsdk
from moviepy.editor import *

# speech_config = speechsdk.SpeechConfig(subscription="da9bf9d5-e8f2-4606-a53d-92cdc0c39248", region="eastasia")
speech_config = speechsdk.SpeechConfig(subscription="2d2dedae6f974f5aa85eac641a0ce74d1", region="eastasia")

def extract_audio(videoPath, audioPath):
    video = VideoFileClip(videoPath)
    audio = video.audio
    audio.write_audiofile(audioPath)


def recognize_from_microphone():
    speech_config.speech_recognition_language="en-US"

    audio_config = speechsdk.audio.AudioConfig(use_default_microphone=True)
    speech_recognizer = speechsdk.SpeechRecognizer(speech_config=speech_config, audio_config=audio_config)

    print("Speak into your microphone.")
    speech_recognition_result = speech_recognizer.recognize_once_async().get()

    if speech_recognition_result.reason == speechsdk.ResultReason.RecognizedSpeech:
        print("Recognized: {}".format(speech_recognition_result.text))
    elif speech_recognition_result.reason == speechsdk.ResultReason.NoMatch:
        print("No speech could be recognized: {}".format(speech_recognition_result.no_match_details))
    elif speech_recognition_result.reason == speechsdk.ResultReason.Canceled:
        cancellation_details = speech_recognition_result.cancellation_details
        print("Speech Recognition canceled: {}".format(cancellation_details.reason))
        if cancellation_details.reason == speechsdk.CancellationReason.Error:
            print("Error details: {}".format(cancellation_details.error_details))
            print("Did you set the speech resource key and region values?")


def speech_recognize_once_from_file(filepath):
    """performs one-shot speech recognition with input from an audio file"""
    # <SpeechRecognitionWithFile>
    audio_config = speechsdk.audio.AudioConfig(filename=filepath)
    # Creates a speech recognizer using a file as audio input, also specify the speech language
    speech_recognizer = speechsdk.SpeechRecognizer(
        speech_config=speech_config, language="zh-CN", audio_config=audio_config)

    # Starts speech recognition, and returns after a single utterance is recognized. The end of a
    # single utterance is determined by listening for silence at the end or until a maximum of 15
    # seconds of audio is processed. It returns the recognition text as result.
    # Note: Since recognize_once() returns only a single utterance, it is suitable only for single
    # shot recognition like command or query.
    # For long-running multi-utterance recognition, use start_continuous_recognition() instead.
    result = speech_recognizer.recognize_once()

    # Check the result
    if result.reason == speechsdk.ResultReason.RecognizedSpeech:
        print("Recognized: {}".format(result.text))
    elif result.reason == speechsdk.ResultReason.NoMatch:
        print("No speech could be recognized: {}".format(result.no_match_details))
    elif result.reason == speechsdk.ResultReason.Canceled:
        cancellation_details = result.cancellation_details
        print("Speech Recognition canceled: {}".format(cancellation_details.reason))
        if cancellation_details.reason == speechsdk.CancellationReason.Error:
            print("Error details: {}".format(cancellation_details.error_details))
    # </SpeechRecognitionWithFile>
    return result.text


def speech_synthesis_to_wave_file(text = " 进上海国际航运中心长三角世界级港口集群打造", file_name = "outputaudio.wav"):
    """performs speech synthesis to a wave file"""
    # Creates a speech synthesizer using file as audio output.
    # Replace with your own audio file name.
    speech_config.speech_synthesis_voice_name='zh-CN-XiaochenNeural'
    # Sets the synthesis language.
    # The full list of supported languages can be found here:
    # https://docs.microsoft.com/azure/cognitive-services/speech-service/language-support#text-to-speech
    
    file_config = speechsdk.audio.AudioOutputConfig(filename=file_name)
    speech_synthesizer = speechsdk.SpeechSynthesizer(speech_config=speech_config, audio_config=file_config)

    result = speech_synthesizer.speak_text_async(text).get()
    # Check result
    if result.reason == speechsdk.ResultReason.SynthesizingAudioCompleted:
        print("Speech synthesized for text [{}], and the audio was saved to [{}]".format(text, file_name))
        return True
    elif result.reason == speechsdk.ResultReason.Canceled:
        cancellation_details = result.cancellation_details
        print("Speech synthesis canceled: {}".format(cancellation_details.reason))
        if cancellation_details.reason == speechsdk.CancellationReason.Error:
            print("Error details: {}".format(cancellation_details.error_details))
        return False

def speech_synthesis_to_mp3_file():
    """performs speech synthesis to a mp3 file"""
    # Sets the synthesis output format.
    # The full list of supported format can be found here:
    # https://docs.microsoft.com/azure/cognitive-services/speech-service/rest-text-to-speech#audio-outputs
    speech_config.set_speech_synthesis_output_format(speechsdk.SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3)

    # Sets the synthesis language.
    # The full list of supported languages can be found here:
    # https://docs.microsoft.com/azure/cognitive-services/speech-service/language-support#text-to-speech
    speech_config.speech_synthesis_language = "zh-CN"
    # Creates a speech synthesizer using file as audio output.
    # Replace with your own audio file name.
    file_name = "outputaudio.mp3"
    file_config = speechsdk.audio.AudioOutputConfig(filename=file_name)
    speech_synthesizer = speechsdk.SpeechSynthesizer(speech_config=speech_config, audio_config=file_config)

    # Receives a text from console input and synthesizes it to mp3 file.
    while True:
        print("Enter some text that you want to synthesize, Ctrl-Z to exit")
        try:
            text = input()
        except EOFError:
            break
        result = speech_synthesizer.speak_text_async(text).get()
        # Check result
        if result.reason == speechsdk.ResultReason.SynthesizingAudioCompleted:
            print("Speech synthesized for text [{}], and the audio was saved to [{}]".format(text, file_name))
        elif result.reason == speechsdk.ResultReason.Canceled:
            cancellation_details = result.cancellation_details
            print("Speech synthesis canceled: {}".format(cancellation_details.reason))
            if cancellation_details.reason == speechsdk.CancellationReason.Error:
                print("Error details: {}".format(cancellation_details.error_details))

def speech_language_detection_once_from_file():
    """performs one-shot speech language detection with input from an audio file"""
    # <SpeechLanguageDetectionWithFile>
    # Creates an AutoDetectSourceLanguageConfig, which defines a number of possible spoken languages
    auto_detect_source_language_config = \
        speechsdk.languageconfig.AutoDetectSourceLanguageConfig(languages=["zh-CN", "en-US"])

    # Creates a SpeechConfig from your speech key and region
    speech_config = speechsdk.SpeechConfig(subscription="2d2dedae6f974f5aa85eac641a0ce74d", region="eastasia")

    # Sets the Priority (optional, defaults to 'Latency'). Either 'Latency' or 'Accuracy' is accepted.
    speech_config.set_property(
        property_id=speechsdk.PropertyId.SpeechServiceConnection_SingleLanguageIdPriority, value='Latency')

    audio_config = speechsdk.audio.AudioConfig(filename=single_language_wav_file)
    # Creates a source language recognizer using a file as audio input, also specify the speech language
    source_language_recognizer = speechsdk.SourceLanguageRecognizer(
        speech_config=speech_config,
        auto_detect_source_language_config=auto_detect_source_language_config,
        audio_config=audio_config)

    # Starts speech language detection, and returns after a single utterance is recognized. The end of a
    # single utterance is determined by listening for silence at the end or until a maximum of 15
    # seconds of audio is processed. It returns the detection text as result.
    # Note: Since recognize_once() returns only a single utterance, it is suitable only for single
    # shot detection like command or query.
    # For long-running multi-utterance detection, use start_continuous_recognition() instead.
    result = source_language_recognizer.recognize_once()

    # Check the result
    if result.reason == speechsdk.ResultReason.RecognizedSpeech:
        print("RECOGNIZED: {}".format(result))
        detected_src_lang = result.properties[
            speechsdk.PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult]
        print("Detected Language: {}".format(detected_src_lang))
    elif result.reason == speechsdk.ResultReason.NoMatch:
        print("No speech could be recognized: {}".format(result.no_match_details))
    elif result.reason == speechsdk.ResultReason.Canceled:
        cancellation_details = result.cancellation_details
        print("Speech Language Detection canceled: {}".format(cancellation_details.reason))
        if cancellation_details.reason == speechsdk.CancellationReason.Error:
            print("Error details: {}".format(cancellation_details.error_details))
    # </SpeechLanguageDetectionWithFile>


def viseme_cb(evt):
    # print("Viseme event received: audio offset: {}ms, viseme id: {}.".format(
        # evt.audio_offset / 10000, evt.viseme_id))
    # `Animation` is an xml string for SVG or a json string for blend shapes
    animation = evt.animation
    print(animation)

def get_bs():
    speech_config.speech_synthesis_language = "zh-CN"
    # speech_config.speech_synthesis_language = "en-US"
    speech_synthesizer = speechsdk.SpeechSynthesizer(speech_config=speech_config)

    # Subscribes to viseme received event
    speech_synthesizer.viseme_received.connect(viseme_cb)

    # ssml = '<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xmlns:mstts="http://www.w3.org/2001/mstts" xml:lang="en-US">\
    ssml = '<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xmlns:mstts="http://www.w3.org/2001/mstts" xml:lang="zh-CN">\
            <voice name="zh-CN-XiaochenNeural">\
                <mstts:viseme type="FacialExpression"/>\
                你是不是傻瓜.\
            </voice>\
            </speak>'

    # If VisemeID is the only thing you want, you can also use `speak_text_async()`
    result = speech_synthesizer.speak_ssml_async(ssml).get()

def synthesis_audio(video_path, output_audio_path):
    audio_path = ""
    extract_audio(video_path, audio_path)
    text = speech_recognize_once_from_file(audio_path)
    speech_synthesis_to_wave_file(text, output_audio_path)


if __name__ == "__main__":
    # speech_recognize_once_from_file()
    get_bs()
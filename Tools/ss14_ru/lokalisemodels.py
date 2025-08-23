import typing
import os
from pydash import py_
from project import Project
import requests
import urllib.parse
import logging

class LocalePath:
    def __init__(self, relative_file_path):
        self.ru = os.path.join(Project().ru_locale_dir_path, relative_file_path)
        self.en = os.path.join(Project().en_locale_dir_path, relative_file_path)


class LokaliseTranslation:
    def __init__(self, data, key_name: typing.AnyStr):
        self.key_name = key_name,
        self.data = data

class LokaliseKey:
    def __init__(self, data):
        self.data = data
        self.key_name = self.data.key_name['web']
        self.key_base_name = self.get_key_base_name(self.key_name)
        self.is_attr = self.check_is_attr()

    def get_file_path(self):

        relative_dir_path = '{relative_file_path}.ftl'.format(
            relative_file_path='/'.join(self.data.key_name['web'].split('.')[0].split('::')))

        return LocalePath(relative_dir_path)

    def get_key_base_name(self, key_name):
        splitted_name = key_name.split('.')
        return splitted_name[0]

    def get_key_last_name(self, key_name):
        splitted_name = key_name.split('.')
        return py_.last(splitted_name)

    def get_parent_key(self):
        if self.is_attr:
            splitted_name = self.key_name.split('.')[0:-1]
            return '.'.join(splitted_name)
        else:
            return None

    def check_is_attr(self):
        return len(self.key_name.split('.')) > 2

    def serialize(self):
        if self.is_attr:
            return self.serialize_attr()
        else:
            return self.serialize_message()



    def serialize_attr(self):
        return '.{name} = {value}'.format(name=self.get_key_last_name(self.key_name), value=self.get_translation('ru').data['translation'])

    def serialize_message(self):
        return '{name} = {value}'.format(name=self.get_key_last_name(self.key_name), value=self.get_translation('ru').data['translation'])

    def get_translation(self, language_iso='ru'):
        filtered = py_.filter(self.data.translations, {'language_iso': language_iso})
        if filtered:
            return LokaliseTranslation(key_name=self.data.key_name['web'], data=filtered[0])

        # Если перевода на русский нет — попробуем взять английский и перевести
        en_translation = py_.find(self.data.translations, {'language_iso': 'en'})
        if en_translation:
            text_to_translate = en_translation['translation']
            translated_text = self.auto_translate(text_to_translate)
            return LokaliseTranslation(key_name=self.data.key_name['web'], data={'translation': translated_text})

        # fallback
        return LokaliseTranslation(key_name=self.data.key_name['web'], data={'translation': '{ "" }'})

    def auto_translate(self, text):
        # Настройка логгера — только один раз (можно вынести в init, если нужно)
        logger = logging.getLogger("autotranslate")
        if not logger.handlers:
            handler = logging.FileHandler("autotranslate.log", mode='a', encoding='utf-8')
            formatter = logging.Formatter('[%(asctime)s] %(message)s', datefmt='%Y-%m-%d %H:%M:%S')
            handler.setFormatter(formatter)
            logger.setLevel(logging.INFO)
            logger.addHandler(handler)

        url = f"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=ru&dt=t&q={urllib.parse.quote(text)}"
        try:
            response = requests.get(url)
            if response.ok:
                result = response.json()
                translated = result[0][0][0]
                logger.info(f'[auto-translate] {text} -> {translated}')
                return translated
            else:
                logger.warning(f'[error] Google returned status {response.status_code} for text: {text}')
                return '{ "" }'
        except Exception as e:
            logger.error(f'[exception] {e} while translating: {text}')
            return '{ "" }'


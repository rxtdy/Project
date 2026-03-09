<template>

  <v-container>

    <h1>Список учебных групп</h1>

    <v-row>

      <v-col cols="12" md="4">
        <v-select
          label="Факультет"
          :items="faculties"
          item-text="text"
          item-value="value"
          v-model="filters.faculty"
          clearable
          @change="loadGroups"
        />
      </v-col>

      <v-col cols="12" md="4">
        <v-select
          label="Форма обучения"
          :items="forms"
          item-text="text"
          item-value="value"
          v-model="filters.form"
          clearable
          @change="loadGroups"
        />
      </v-col>

      <v-col cols="12" md="4">
        <v-select
          label="Уровень образования"
          :items="levels"
          item-text="text"
          item-value="value"
          v-model="filters.level"
          clearable
          @change="loadGroups"
        />
      </v-col>

      <v-col cols="12" md="4">
        <v-select
          label="Курс"
          :items="courses"
          v-model="filters.courses"
          multiple
          clearable
          @change="loadGroups"
        />
      </v-col>

      <v-col cols="12" md="4">
        <v-select
          label="Учебный год"
          :items="years"
          v-model="filters.year"
          clearable
          @change="loadGroups"
        />
      </v-col>

    </v-row>

    <v-data-table
    :headers="headers"
    :items="groups"
    >

      <template v-slot:no-data>
      Нет данных по выбранным фильтрам
      </template>

    </v-data-table>

  </v-container>

</template>

<script>

import axios from "axios";

export default {
  data() {
    return {
      groups: [],

      filters: {
      faculty: null,
      form: null,
      level: null,
      courses: [],
      year: null
      },

      faculties: [
        { text: "Факультет информатики", value: 1 },
        { text: "Факультет экономики", value: 2 },
        { text: "Факультет медицины", value: 3 },
        { text: "Факультет гуманитарных наук", value: 4 },
        { text: "Факультет физики", value: 5 }
      ],
      forms: [
        { text: "Очная форма", value: 1 },
        { text: "Заочная форма", value: 2 },
        { text: "Очно-заочная форма", value: 3 }
      ],
      levels: [
        { text: "Специалитет", value: 1 },
        { text: "Бакалавриат", value: 2 },
        { text: "Магистратура", value: 3 },
        { text: "Аспирантура", value: 4 },
        { text: "Ординатура", value: 5 }
      ],
      years: ["2023-2024", "2024-2025", "2025-2026"],
      courses: [1,2,3,4,5,6],

      headers: [
      { text:"Название группы", value:"названиеГруппы"},
      { text:"Факультет", value:"факультет"},
      { text:"Специальность", value:"специальность"},
      { text:"Курс", value:"курс"},
      { text:"Форма обучения", value:"формаОбучения"},
      { text:"Уровень обучения", value:"уровеньОбучения"},
      { text:"Учебный год", value:"учебныйГод"}
      ]

      };

  },

  mounted(){
    this.loadGroups();
  },

  methods:{
    async loadGroups(){

    let params = {
      факультет: this.filters.faculty,
      формаОбучения: this.filters.form,
      уровеньОбучения: this.filters.level,
      учебныйГод: this.filters.year,
      курсы: this.filters.courses.length > 0 ? this.filters.courses.join(',') : null
    };

    let response = await axios.get("https://localhost:7222/groups",{ params });

    this.groups = response.data;

    }

  }

};

</script>
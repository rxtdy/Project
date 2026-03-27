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
          item-text="text"
          item-value="value"
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
          item-text="text"
          item-value="value"
          v-model="filters.year"
          clearable
          @change="loadGroups"
        />
      </v-col>
    </v-row>

    <v-skeleton-loader
      v-if="loading.groups"
      type="table@5"  
      class="mt-4"
    />

    <v-data-table
      v-else
      :headers="headers"
      :items="groups"
      class="mt-4"
    >
      <template #no-data>
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
      
      faculties: [],
      forms: [],
      levels: [],
      courses: [],
      years: [],

      filters: {
        faculty: null,
        form: null,
        level: null,
        courses: [],
        year: null
      },

      loading: {
        groups: false
      },

      headers: [
        { text: "Название группы", value: "названиеГруппы" },
        { text: "Факультет", value: "факультет" },
        { text: "Специальность", value: "специальность" },
        { text: "Курс", value: "курс" },
        { text: "Форма обучения", value: "формаОбучения" },
        { text: "Уровень обучения", value: "уровеньОбучения" },
        { text: "Учебный год", value: "учебныйГод" }
      ]
    };
  },

  mounted() {
    this.loadAllFilters();
    this.loadGroups();
  },

  methods: {
    async loadAllFilters() {
      try {
        const response = await axios.get("https://localhost:7222/filters");
        const data = response.data;
        
        this.faculties = data.faculties || [];
        this.forms = data.forms || [];
        this.levels = data.levels || [];
        this.courses = data.courses || [];
        this.years = data.years || [];
      } catch (error) {
        console.error("Ошибка загрузки фильтров:", error);
      }
    },

    async loadGroups() {
      this.loading.groups = true;
      try {
        let params = {
          факультет: this.filters.faculty,
          формаОбучения: this.filters.form,
          уровеньОбучения: this.filters.level,
          учебныйГод: this.filters.year,
          курсы: this.filters.courses.length > 0 ? this.filters.courses.join(',') : null
        };

        let response = await axios.get("https://localhost:7222/groups", { params });
        this.groups = response.data;
      } finally {
        this.loading.groups = false;
      }
    }
  }
};
</script>
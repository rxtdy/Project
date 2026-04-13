<template>
  <v-container>
    <h1 class="mb-4">Список учебных групп</h1>
    
    <v-row>
      <v-col cols="12" md="2">
        <v-select label="Факультет" :items="faculties" item-text="text" item-value="value" 
          v-model="filters.faculty" clearable @change="triggerSearch"></v-select>
      </v-col>
      <v-col cols="12" md="2">
        <v-select label="Форма обучения" :items="forms" item-text="text" item-value="value" 
          v-model="filters.form" clearable @change="triggerSearch"></v-select>
      </v-col>
      <v-col cols="12" md="3">
        <v-select label="Уровень образования" :items="levels" item-text="text" item-value="value" 
          v-model="filters.level" clearable @change="triggerSearch"></v-select>
      </v-col>
      <v-col cols="12" md="3">
        <v-select label="Курс" :items="courses" item-text="text" item-value="value" 
          v-model="filters.courses" multiple clearable @change="triggerSearch"></v-select>
      </v-col>
      <v-col cols="12" md="2">
        <v-select label="Учебный год" :items="years" item-text="text" item-value="value" 
          v-model="filters.year" clearable @change="triggerSearch"></v-select>
      </v-col>
    </v-row>

    <v-data-table
      :headers="headers"
      :items="groups"
      :options.sync="internalOptions"
      :server-items-length="totalGroups"
      :loading="loading"
      :footer-props="{
        'items-per-page-options': [5, 10, 20, 50],
        'items-per-page-text': 'Записей на странице:',
        'page-text': '{0}-{1} из {2}'
      }"
      class="elevation-1 mt-4"
    >
      <template v-slot:[`item.названиеГруппы`]="{ item }">
        <span class="group-link primary--text font-weight-bold" @click="openGroup(item)">
          {{ item.названиеГруппы }}
        </span>
      </template>
    </v-data-table>
  </v-container>
</template>

<script>
import axios from 'axios';
const API_URL = "https://localhost:7222";

export default {
  name: 'GroupsTable',
  props: {
    page: { type: Number, default: 1 },
    pageSize: { type: Number, default: 10 }
  },
  data() {
    return {
      groups: [],
      totalGroups: 0,
      loading: false,
      internalOptions: { page: this.page, itemsPerPage: this.pageSize },
      faculties: [], forms: [], levels: [], courses: [], years: [],
      filters: { faculty: null, form: null, level: null, courses: [], year: null },
      headers: [
        { text: "Название группы", value: "названиеГруппы", sortable: false },
        { text: "Факультет", value: "факультет", sortable: false },
        { text: "Специальность", value: "специальность", sortable: false },
        { text: "Курс", value: "курс", sortable: false },
        { text: "Форма обучения", value: "формаОбучения", sortable: false },
        { text: "Уровень", value: "уровеньОбучения", sortable: false },
        { text: "Год", value: "учебныйГод", sortable: false }
      ]
    };
  },
  watch: {
    internalOptions: {
      handler(newVal) {
        if (newVal.page !== this.page || newVal.itemsPerPage !== this.pageSize) {
          this.$router.push({
            query: { 
              ...this.$route.query, 
              page: newVal.page, 
              pageSize: newVal.itemsPerPage 
            }
          }).catch(() => {});
        }
      },
      deep: true
    },
    page() {
      this.internalOptions.page = this.page;
      this.loadGroups();
    },
    pageSize() {
      this.internalOptions.itemsPerPage = this.pageSize;
      this.loadGroups();
    },
  },

  mounted() { this.loadAllFilters(); this.loadGroups();}, 

  methods: {
    triggerSearch() {
      this.internalOptions.page = 1;
      this.loadGroups();
      this.loadAllFilters();
    },
    async loadAllFilters() {
      try {
        const params = { 
          факультет: this.filters.faculty,
          формаОбучения: this.filters.form,
          уровеньОбучения: this.filters.level,
          учебныйГод: this.filters.year,
          курсы: this.filters.courses.length > 0 ? this.filters.courses.join(',') : null
        };
        
        const response = await axios.get(`${API_URL}/filters`, { params });
        Object.assign(this, response.data);
      } catch (error) { 
        console.error(error); 
      }
    },
    async loadGroups() {
      this.loading = true;
      try {
        const params = { 
          факультет: this.filters.faculty,
          формаОбучения: this.filters.form,
          уровеньОбучения: this.filters.level,
          учебныйГод: this.filters.year,
          курсы: this.filters.courses.length > 0 ? this.filters.courses.join(',') : null,
          page: this.page, 
          pageSize: this.pageSize 
        };
        const { data } = await axios.get(`${API_URL}/groups`, { params });
        this.groups = data.items;
        this.totalGroups = data.total;
      } finally { this.loading = false; }
    },
    openGroup(item) {
      this.$router.push({ 
        name: 'Students', 
        params: { id: item.groupId }, 
        query: { 
          ...this.$route.query, 
          groupName: item.названиеГруппы
        }
      });
    }
  }
};
</script>

<style scoped> 
.group-link { text-decoration: underline; cursor: pointer; } 
.group-link:hover { color: #1565C0 !important; }
</style>
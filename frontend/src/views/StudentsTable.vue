<template>
  <v-container>
    <v-row align="center" class="mb-2">
      <v-col cols="auto">
        <v-btn icon @click="$router.push({ path: '/', query: $route.query })">
          <v-icon>mdi-arrow-left</v-icon>
        </v-btn>
      </v-col>
      <v-col>
        <h1>Студенты группы: {{ groupName || id }}</h1>
        <p class="small font-weight-thin" v-if="groupName">
          id: {{ id }}
        </p>
      </v-col>
      <v-col cols="auto">
        <v-btn v-if="!isEditing" color="primary" @click="startEdit">Редактировать</v-btn>
        <v-btn v-if="isEditing" color="error" text @click="cancelEdit" class="mr-2">Отмена</v-btn>
        <v-btn v-if="isEditing" color="success" @click="saveChanges" :loading="saving">Сохранить</v-btn>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="3">
        <v-select label="Пол" :items="genderOptions" item-text="text" item-value="value" 
          v-model="filters.gender" clearable @change="loadStudents"></v-select>
      </v-col>
      <v-col cols="12" md="3">
        <v-select label="Статус" :items="statusOptions" item-text="text" item-value="value" 
          v-model="filters.status" clearable @change="loadStudents"></v-select>
      </v-col>
    </v-row>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" top>
      {{ snackbar.text }}
      <template v-slot:action="{ attrs }">
        <v-btn text v-bind="attrs" @click="snackbar.show = false">Закрыть</v-btn>
      </template>
    </v-snackbar>

    <v-data-table
      :headers="headers"
      :items="students"
      :items-per-page="10"
      :loading="loading"
      :footer-props="{
        'items-per-page-options': [5, 10, -1],
        'items-per-page-text': 'Записей на странице:',
        'page-text': '{0}-{1} из {2}'
      }"
      class="elevation-1 mt-4"
    >
      <template v-slot:[`item.fio`]="{ item }">
        <v-text-field v-if="isEditing" v-model="item.fio" dense hide-details outlined></v-text-field>
        <span v-else>{{ item.fio }}</span>
      </template>
      
      <template v-slot:[`item.status`]="{ item }">
        <v-chip :color="getStatusColor(item.status)" dark small>
          {{ getStatusText(item.status) }}
        </v-chip>
      </template>

      <template #no-data>
        Студенты не найдены
      </template>
    </v-data-table>
  </v-container>
</template>

<script>
import axios from 'axios';
const API_URL = "https://localhost:7222";

export default {
  name: 'StudentsTable',
  props: {
    id: { required: true },
    groupName: { type: String, default: '' },
  },
  data() {
    return {
      students: [],
      originalStudents: [],
      loading: false,
      saving: false,
      isEditing: false,
      filters: { gender: null, status: null },
      genderOptions: [
        { text: 'Мужской', value: 'Муж' },
        { text: 'Женский', value: 'Жен' }
      ],
      statusOptions: [
        { text: 'Отчислен', value: 0 },
        { text: 'Учится', value: 1 },
        { text: 'Выпускник', value: 2 },
        { text: 'Академический отпуск', value: 3 }
      ],
      snackbar: { show: false, text: '', color: 'success' },
      headers: [
        { text: "ФИО", value: "fio", sortable: false, width: "40%" },
        { text: "Номер Зачетки", value: "recordBook", sortable: false },
        { text: "Пол", value: "gender", sortable: false },
        { text: "Средний балл", value: "gpa", sortable: false },
        { text: "Статус", value: "status", sortable: false }
      ]
    };
  },

  mounted() {
    this.loadStudents();
  },

  methods: {
    async loadStudents() {
      this.loading = true;
      try {
        const params = {
          groupId: this.id,
          пол: this.filters.gender,
          статус: this.filters.status,
        };
        const { data } = await axios.get(`${API_URL}/students`, { params });
        this.students = data.items;
        this.originalStudents = JSON.parse(JSON.stringify(this.students));
      } catch (error) {
        this.showSnack("Ошибка загрузки", "error");
      } finally {
        this.loading = false;
      }
    },
    getStatusText(status) {
      const opt = this.statusOptions.find(o => o.value === status);
      return opt ? opt.text : 'Неизвестно';
    },
    getStatusColor(status) {
      switch(status) {
        case 0: return 'error';
        case 1: return 'success';
        case 2: return 'primary';
        case 3: return 'warning';
        default: return 'grey';
      }
    },
    startEdit() { this.isEditing = true; },
    cancelEdit() {
      this.isEditing = false;
      this.students = JSON.parse(JSON.stringify(this.originalStudents));
    },
    async saveChanges() {
      this.saving = true;
      try {
        const updates = this.students.filter((student, index) => {
          return student.fio !== this.originalStudents[index].fio;
        }).map(s => ({ id: s.id, fio: s.fio }));

        if (updates.length > 0) {
          await axios.put(`${API_URL}/students`, updates);
          this.showSnack(`Обновлено студентов: ${updates.length}`, "success");
        }
        this.isEditing = false;
        this.loadStudents();
      } catch (error) {
        this.showSnack("Ошибка сохранения", "error");
      } finally {
        this.saving = false;
      }
    },
    showSnack(text, color) {
      this.snackbar.text = text;
      this.snackbar.color = color;
      this.snackbar.show = true;
    }
  }
};
</script>
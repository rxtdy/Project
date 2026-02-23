<template>
      <v-container>
        <h1>Список учебных групп</h1>
        <v-data-table
          :headers="headers"
          :items="groups"
          ></v-data-table>
      </v-container>
    </template>

    <script>
    import axios from "axios";

    export default {
      data() {
        return {
          groups: [],
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
        axios
          .get("https://localhost:7222/groups")
          .then(response => {
            this.groups = response.data;
          });
      }
    };
    </script>
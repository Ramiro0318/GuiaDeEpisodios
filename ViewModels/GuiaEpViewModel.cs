using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuiaDeEpisodios.Models;
using MvvmCross.Binding.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace GuiaDeEpisodios.ViewModels
{
    public enum Ventanas
    {
        Temporadas, AgregarEpisodio, AgregarTemporada, EditarEpisodio,
        EditarTemporada, Eliminar, Episodios
    };

    public class GuiaEpViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Episodio> Episodios { get; set; } = new();
        public ObservableCollection<Temporada> Temporadas { get; set; } = new();

        public ICommand VerAgregarEpisodioCommand { set; get; }
        public ICommand VerAgregarTemporadaCommand { set; get; }

        public ICommand AgregarEpisodioCommand { set; get; }
        public ICommand AgregarTemporadaCommand { set; get; }



        public ICommand VerEditarEpisodioCommand { set; get; }
        public ICommand VerEditarTemporadaCommand { set; get; }
        public ICommand EditarEpisodioCommand { set; get; }
        public ICommand EditarTemporadaCommand { set; get; }

        public ICommand VerEliminarCommand { set; get; }
        public ICommand EliminarEpisodioCommand { set; get; }
        public ICommand EliminarTemporadaCommand { set; get; }

        public ICommand CancelarCommand { set; get; }
        public ICommand VerEpisodiosCommand { set; get; }

        public Ventanas Ventana { get; set; } = Ventanas.Temporadas;
        public Ventanas VentanaAnteriorABorrar { get; set; }

        public Episodio episodio { get; set; } = null!;
        public Temporada temporada { get; set; } = null!;

        public string Error { get; set; } = "";

        public GuiaEpViewModel()
        {
            VerAgregarEpisodioCommand = new RelayCommand(VerAgregarEpisodio);
            VerAgregarTemporadaCommand = new RelayCommand(VerAgregarTemporada);

            AgregarEpisodioCommand = new RelayCommand(AgregarEpisodio);
            AgregarTemporadaCommand = new RelayCommand(AgregarTemporada);


            VerEditarTemporadaCommand = new RelayCommand(VerEditarTemporada);
            VerEditarEpisodioCommand = new RelayCommand(VerEditarCapitulo);

            EditarEpisodioCommand = new RelayCommand(EditarEpisodio);
            EditarTemporadaCommand = new RelayCommand(EditarTemporada);

            VerEliminarCommand = new RelayCommand(VerEliminar);

            EliminarEpisodioCommand = new RelayCommand(EliminarEpisodio);
            EliminarTemporadaCommand = new RelayCommand(EliminarTemporada);

            CancelarCommand = new RelayCommand(Cancelar);
            VerEpisodiosCommand = new RelayCommand(VerEpisodios);

            Cargar();
        }

        private void VerEpisodios()
        {
            if (temporada != null)
            {
                var cantidadEpEliminar = (from a in Episodios where a.Temporada != temporada.Nombre select a).Count();

                if (cantidadEpEliminar > 0)
                {
                    for (int i = 0; i < cantidadEpEliminar; i++)
                    {
                        Episodio? episodioEliminar = (from x in Episodios where x.Temporada != temporada.Nombre select x).First();
                        Episodios.Remove(episodioEliminar);
                    }
                }
            }

            Ventana = Ventanas.Episodios;
            VentanaAnteriorABorrar = Ventanas.Episodios;
            Actualizar(nameof(Ventana));

        }

        int indexTemporadaAnterior;

        private void VerEditarTemporada()
        {
            Error = "";

            var clonTemporada = new Temporada()
            {
                Nombre = temporada.Nombre,
                Imagen = temporada.Imagen,
                Siposis = temporada.Siposis,
            };

            indexTemporadaAnterior = Temporadas.IndexOf(temporada);
            temporada = clonTemporada;

            Ventana = Ventanas.EditarTemporada;
            Actualizar(nameof(Ventana));
        }

        int indexEpisodioAnterior;

        private void VerEditarCapitulo()
        {
            Error = "";
            var clonEpisodio = new Episodio()
            {
                Nombre = episodio.Nombre,
                ImagenEp = episodio.ImagenEp,
                Descripcion = episodio.Descripcion,
                Temporada = episodio.Temporada,
                Index = episodio.Index
            };

            indexEpisodioAnterior = Episodios.IndexOf(episodio);
            Episodios.Clear();
            Temporadas.Clear();
            Cargar();

            episodio = clonEpisodio;

            Ventana = Ventanas.EditarEpisodio;
            Actualizar(nameof(Ventana));
        }
        private void EditarTemporada()
        {
            Error = "";
            if (temporada != null)
            {
                
                
                if (string.IsNullOrWhiteSpace(temporada.Nombre))
                {
                    Error = "Escriba el nombre de la temporada";
                    Actualizar(nameof(Error));
                    return;
                }
                if (string.IsNullOrWhiteSpace(temporada.Imagen) || !temporada.Imagen.EndsWith(".jpg"))
                //!temporada.Imagen.StartsWith("http")

                {
                    Error = "Escriba la direccion de imagen con fotmato .jpg";
                    Actualizar(nameof(Error));
                    return;
                }
                //if (string.IsNullOrWhiteSpace(temporada.Siposis))
                //{
                //    Error = "Escriba el nombre de la temporada";
                //    Actualizar(nameof(Error));
                //    return;
                //}
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    Actualizar(nameof(Error));
                    return;
                }


                var cantidadEpEditar = (from a in Episodios where a.Temporada == Temporadas[indexTemporadaAnterior].Nombre select a).Count();

                if (cantidadEpEditar > 0)
                {
                    for (int i = 0; i < cantidadEpEditar; i++)
                    {
                        Episodio? episodioEditar = (from x in Episodios where x.Temporada == Temporadas[indexTemporadaAnterior].Nombre select x).First();
                        episodioEditar.Temporada = temporada.Nombre;
                    }
                }

                Temporadas[indexTemporadaAnterior] = temporada;
                var duplicado = (from x in Temporadas where x.Nombre == temporada.Nombre select x).Count();
                if (duplicado > 1)
                {
                    Error = "El nombre de la temporada no puede estar duplicado";
                    Actualizar(nameof(Error));
                    return;
                }
                Guardar();
                Ventana = Ventanas.Temporadas;
                Actualizar(nameof(Ventana));
              
            }
        }

        private void EditarEpisodio()
        {
            Error = "";
            
            if (episodio != null)
            {  
                if (string.IsNullOrWhiteSpace(episodio.Nombre))
                {
                    Error = "Escriba el nombre del capitulo";
                    Actualizar(nameof(Error));
                    return;
                }
                if (string.IsNullOrWhiteSpace(episodio.ImagenEp) || //!episodio.ImagenEp.StartsWith("http")
                    !episodio.ImagenEp.EndsWith(".jpg"))
                {
                    Error = "Escriba la direccion de imagen con formato .jpg";
                    Actualizar(nameof(Error));
                    return;
                }
                if (string.IsNullOrWhiteSpace(episodio.Descripcion))
                {
                    Error = "Coloque una descripcion al episodio";
                    Actualizar(nameof(Error));
                    return;
                }
                if (string.IsNullOrWhiteSpace(episodio.Temporada))
                {
                    Error = "Seleccione la temporada a la que pertenece el episodio";
                    Actualizar(nameof(temporada));
                    return;
                }
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    Actualizar(nameof(Error));
                    return;
                } 

                var remover = (from x in Episodios where x.Nombre == episodio.Nombre select x ).First();
                Episodios.Remove(remover);

                Episodios[indexEpisodioAnterior] = episodio;
                episodio.Temporada = Temporadas[episodio.Index].Nombre;
               

                //Guardar();
                Guardar();
                Ventana = Ventanas.Temporadas;
                Actualizar(nameof(Ventana));
            }
        }
        private void VerEliminar()
        {
            Ventana = Ventanas.Eliminar;
            Actualizar(nameof(Ventana));
        }

        private void EliminarTemporada()
        {
            if (temporada != null)
            {
                var cantidadEpEliminar = (from a in Episodios where a.Temporada == temporada.Nombre select a).Count();
                Temporadas.Remove(temporada);
                if (cantidadEpEliminar > 0)
                {
                    for (int i = 0; i < cantidadEpEliminar; i++)
                    {
                        Episodio? episodioEliminar = (from x in Episodios where x.Temporada == temporada.Nombre select x).First();
                        Episodios.Remove(episodioEliminar);
                    }
                }
                Guardar();
                Ventana = Ventanas.Temporadas;
                Actualizar(nameof(Ventana));

            }
        }

        private void EliminarEpisodio()
        {
            if (episodio != null)
            {
                Episodios.Remove(episodio);
                Guardar();
                Ventana = Ventanas.Temporadas;
                Actualizar(nameof(Ventana));
            }
        }


        private void VerAgregarTemporada()
        {
            temporada = new();
            Ventana = Ventanas.AgregarTemporada;
            Actualizar(nameof(Ventana));
        }
        private void AgregarTemporada()
        {
            Error = "";
            bool isDuplicado = (from x in Temporadas where x.Nombre == temporada.Nombre select x).Any();
            if (temporada != null)
            {
                if (string.IsNullOrWhiteSpace(temporada.Nombre))
                {
                    Error = "Escriba el nombre de la temporada";
                    Actualizar(nameof(Error));
                    return;
                }
                if (string.IsNullOrWhiteSpace(temporada.Imagen) || !temporada.Imagen.EndsWith(".jpg"))
                //!temporada.Imagen.StartsWith("http")

                {
                    Error = "Escriba la direccion de imagen con fotmato .jpg";
                    Actualizar(nameof(Error));
                    return;
                }
                if (isDuplicado)
                {
                    Error = "Una temporada con este nombre ya eiste";
                    Actualizar(nameof(Error));
                    return;
                }
                //if (string.IsNullOrWhiteSpace(temporada.Siposis))
                //{
                //    Error = "Escriba el nombre de la temporada";
                //    Actualizar(nameof(Error));
                //    return;
                //}
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    Actualizar(nameof(Error));
                    return;
                }

                Temporadas.Add(temporada);
                Guardar();
                Ventana = Ventanas.Temporadas;
                Actualizar(nameof(Ventana));
            }
        }


        private void VerAgregarEpisodio()
        {
            episodio = new();
            Ventana = Ventanas.AgregarEpisodio;
            Actualizar(nameof(Ventana));
        }

        private void AgregarEpisodio() ///////////////////
        {
            Error = "";
            if (episodio != null)
            {
                if (string.IsNullOrWhiteSpace(episodio.Nombre))
                {
                    Error = "Escriba el nombre del capitulo";
                    Actualizar(nameof(Error));
                    return;
                }
                if (string.IsNullOrWhiteSpace(episodio.ImagenEp)|| //!episodio.ImagenEp.StartsWith("http")
                    !episodio.ImagenEp.EndsWith(".jpg"))
                {
                    Error = "Escriba la direccion de imagen con formato .jpg";
                    Actualizar(nameof(Error));
                    return;
                }
                if (string.IsNullOrWhiteSpace(episodio.Descripcion))
                {
                    Error = "Coloque una descripcion al episodio";
                    Actualizar(nameof(Error));
                    return;
                }
                if (string.IsNullOrWhiteSpace(episodio.Temporada))
                {
                    Error = "Seleccione la temporada a la que pertenece el episodio";
                    Actualizar(nameof(temporada));
                    return;
                }
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    Actualizar(nameof(Error));
                    return;
                }

                episodio.Temporada = Temporadas[episodio.Index].Nombre;
                episodio.Temporada.ToString();
                Episodios.Add(episodio);
                Guardar();
                Ventana = Ventanas.Temporadas;
                Actualizar(nameof(Ventana));
            }
        }

        private void Cancelar()
        {
            if (VentanaAnteriorABorrar == Ventanas.Episodios)
            {
                Episodios.Clear();
                Temporadas.Clear();
                Cargar();
            }
            Ventana = Ventanas.Temporadas;
            VentanaAnteriorABorrar = Ventanas.Temporadas;
            Actualizar(nameof(Ventana));
        }

        const string episodios = "Episodios.json";
        const string temporadas = "Temporadas.json";

        private void Guardar()
        {
            File.WriteAllText(episodios, JsonSerializer.Serialize(Episodios));
            File.WriteAllText(temporadas, JsonSerializer.Serialize(Temporadas));
        }

        private void Cargar()
        {
            try
            {
                if (File.Exists(episodios))
                {
                    var ep =
                    JsonSerializer.Deserialize<ObservableCollection<Episodio>>(File.ReadAllText(episodios));
                    if (ep != null)
                    {
                        foreach (var episodio in ep)
                        {
                            Episodios.Add(episodio);
                        }
                    }
                }

                if (File.Exists(temporadas))
                {
                    var temp =
                    JsonSerializer.Deserialize<ObservableCollection<Temporada>>(File.ReadAllText(temporadas));
                    if (temp != null)
                    {
                        foreach (var temporada in temp)
                        {
                            Temporadas.Add(temporada);
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Actualizar(string? nameof)
        {
            PropertyChanged?.Invoke(this, new(nameof));

        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    //Ramiro Valverde
}

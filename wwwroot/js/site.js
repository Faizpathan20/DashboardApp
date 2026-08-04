// ---------------------------------------
// HERO SLIDER
// ---------------------------------------

const heroSlides = document.querySelectorAll(".hero-slide");

let currentHeroSlide = 0;


function showHeroSlide(index) {

    heroSlides.forEach(slide => {
        slide.classList.remove("active");
    });


    if(heroSlides[index]){
        heroSlides[index].classList.add("active");
    }

}


// Auto Slide Every 4 Seconds

if(heroSlides.length > 0){

    setInterval(()=>{

        currentHeroSlide++;

        if(currentHeroSlide >= heroSlides.length){
            currentHeroSlide = 0;
        }

        showHeroSlide(currentHeroSlide);


    },4000);

}


// Manual dots click

function changeSlide(index){

    currentHeroSlide = index;

    showHeroSlide(currentHeroSlide);

}





// ---------------------------------------
// SEARCH
// ---------------------------------------

const searchInput =
    document.getElementById("searchInput");


if (searchInput) {

    searchInput.addEventListener("keyup", function () {


        const searchValue =
            this.value.toLowerCase();


        const rows =
            document.querySelectorAll(
                "#dataTable tbody tr"
            );


        rows.forEach(row => {


            const text =
                row.innerText.toLowerCase();


            row.style.display =
                text.includes(searchValue)
                    ? ""
                    : "none";


        });


    });

}





// ---------------------------------------
// QUANTITY CHART
// ---------------------------------------


const dateMap = {};


if(typeof records !== "undefined"){


    records.forEach(item => {


        const date =
            item.DocDate || "Unknown";


        if (!dateMap[date]) {

            dateMap[date] = 0;

        }


        dateMap[date] += Number(item.Qty || 0);


    });



    const dates =
        Object.keys(dateMap).slice(-15);



    const quantities =
        dates.map(date => dateMap[date]);



    const chartElement =
        document.getElementById("quantityChart");



    if (chartElement) {


        new Chart(chartElement, {


            type: "line",


            data: {


                labels: dates,


                datasets: [


                    {

                        label: "Quantity",


                        data: quantities,


                        borderWidth: 3,


                        tension: 0.35,


                        fill: false

                    }


                ]


            },


            options: {


                responsive:true,


                maintainAspectRatio:false,


                plugins:{


                    legend:{


                        display:false


                    }


                },


                scales:{


                    y:{


                        beginAtZero:true


                    }


                }


            }


        });


    }


}
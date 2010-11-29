(function($){
	$.statQueryWidget = function (target,url,loadingCallback,okCallback,errorCallback) {
		var sq = new StatQueryWidget (target,url,loadingCallback,okCallback,errorCallback);
		sq.fillArgs();
		sq.updatePlot();
		return sq;
	}
	
	function StatQueryWidget (target,url,loadingCallback,okCallback,errorCallback)
	{
		sq = this;
		this.loadingCallback = loadingCallback;
		this.okCallback = okCallback;
		this.errorCallback = errorCallback;
		if (url instanceof Array)
			this.queryUrls = url;
		else
			this.queryUrls = [ url ];
		
		var t = $("#" + target);
		var h = "Stats for the ";
		h += "<select id='" + target + "_periodSelect'>";
		h += "<option value='month'>month</option>";
		h += "<option value='year'>year</option>";
		h += "<option value='period'>date range</option>";
		h += "<option value='last'>last...</option>";
		h += "</select> ";
		
		h += "<span id='" + target + "_plotLastSection'>";
		h += "<input id='" + target + "_plotNumArg' type='text' style='width:50px'/> ";
		h += "<select id='" + target + "_plotLastSectionSel'>";
		h += "<option value='d'>days</option>";
		h += "<option value='w'>weeks</option>";
		h += "<option value='m'>months</option>";
		h += "<option value='y'>years</option>";
		h += "</select>";
		h += "</span> ";
		
		h += "<span id='" + target + "_plotPeriodSection'>";
		h += "starting on <input type='text' id='" + target + "_plotDatepicker1' style='width:100px'/> until <input type='text' style='width:100px' id='" + target + "_plotDatepicker2'/>";
		h += "</span> ";
		
		h += "<span id='" + target + "_plotValueSection'>";
		h += "<select id='" + target + "_plotValueSectionSel'>";
		h += "</select>";
		h += "</span> ";
		
		h += "<input id='" + target+ "_updatePlotButton' type='button' value='Update'/>";
		t.append (h);
		
		this._periodSelect = $("#" + target + "_periodSelect");
		this._plotLastSection = $("#" + target + "_plotLastSection");
		this._plotNumArg = $("#" + target + "_plotNumArg");
		this._plotLastSectionSel = $("#" + target + "_plotLastSectionSel");
		this._plotPeriodSection = $("#" + target + "_plotPeriodSection");
		this._plotDatepicker1 = $("#" + target + "_plotDatepicker1");
		this._plotDatepicker2 = $("#" + target + "_plotDatepicker2");
		this._plotValueSection = $("#" + target + "_plotValueSection");
		this._plotValueSectionSel = $("#" + target + "_plotValueSectionSel");
		this._updatePlotButton = $("#" + target + "_updatePlotButton");
		
		// Initialize controls
		
		this._plotDatepicker1.datepicker();
		this._plotDatepicker2.datepicker();
		
		this._periodSelect.val ("last");
		this._plotNumArg.val ("30");
		this._plotLastSectionSel.val ("d");
		
		this._periodSelect.change (function () {
			sq.fillArgs();
		});
						
		this._updatePlotButton.click (function () {
			sq.updatePlot ();
		});
		
		this.fillArgs = function () {
			this._plotPeriodSection.hide();
			this._plotLastSection.hide();
			this._plotValueSection.hide();
		
			var period = this._periodSelect.val ();
			if (period == "period") {
				this._plotPeriodSection.show();
			}
			else if (period == "last") {
				this._plotLastSection.show();
			}
			else if (period == "month") {
				this._plotValueSection.show();
				options = "";
				var months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
				var td = new Date ();
				m = td.getMonth();
				y = td.getFullYear() - 1;
				for (n=0; n<12; n++) {
					options += "<option value='1-" + (m+1) + "-" + y + "'>";
					options += months[m] + " " + y + "</option>";
					if (++m == 12) {
						m = 0; y++;
					}
				}
				this._plotValueSectionSel.html(options);
			}
			else if (period == "year") {
				this._plotValueSection.show();
				options = "";
				var td = new Date ();
				y = td.getFullYear();
				for(n=y; n>=y-5; n--)
					options += "<option value='" + n + "'>" + n + "</option>";
				this._plotValueSectionSel.html(options);
			}
		}
		
		this.updatePlot = function ()	{
			var period = this._periodSelect.val ();
			if (period == "period") {
				arg = this._plotDatepicker1.val() + "." + this._plotDatepicker2.val()
			}
			else if (period == "last") {
				arg = this._plotNumArg.val() + this._plotLastSectionSel.val();
			}
			else if (period == "month" || period == "year") {
				arg = this._plotPeriodSection.val();
			}
			this.loadingCallback();
			this.queryError = false;
			var ob = this;
			
			for (n=0; n<this.queryUrls.length; n++) {
				var q = new Object ();
				q.idx = n;
				$.ajax ({
					url: this.queryUrls[n] + "period=" + period + "&arg=" + arg,
					dataType: "json",
					context: q,
					success: function (data) {
						if (ob.queryUrls.length == 1)
							ob.okCallback (data);
						else
							ob.okCallback (data, this.idx);
					},
					error: function () {
						if (!ob.queryError) {
							ob.queryError = true;
							ob.errorCallback();
						}
					}
				});
			}
		}
	}
})(jQuery)


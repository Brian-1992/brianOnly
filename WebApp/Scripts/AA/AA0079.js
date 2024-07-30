Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Set = '../../../api/AA0079/SetBcItmanagers'; // 新增/修改/刪除
    var T1Name = "藥庫一~四級管制藥品出入帳明細月報表";

    var reportUrl = '/Report/A/AA0079.aspx';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    //function setPrintType() {
    //    printType = Ext.getUrlParam('printtype');
    //    
    //    if (printType == "frwh") {  // AA0087 只有轉出選項
    //        
    //        Ext.getCmp('radio2').hide();
    //    } else {
    //        Ext.getCmp('radio2').show();
    //    }
    //}

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });
    function setWhnoComboData() {
        Ext.Ajax.request({
            url: '/api/AA0079/GetWhnoCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add({ WH_NO: wh_nos[i].WH_NO, WH_NAME: wh_nos[i].WH_NAME });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setWhnoComboData();

    // 庫房級別清單
    var whgradeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['GRADE_VALUE', 'GRADE_NAME']
    });
    function setWhgradeComboData() {
        Ext.Ajax.request({
            url: '/api/AA0079/GetWhgradeCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var grades = data.etts;
                    
                    if (grades.length > 0) {
                        for (var i = 0; i < grades.length; i++) {
                            whgradeQueryStore.add({ GRADE_VALUE: grades[i].GRADE_VALUE, GRADE_NAME: grades[i].GRADE_NAME });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setWhgradeComboData();


    

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'radiogroup',
                    name: 'condition1',
                    fieldLabel: '查詢條件1',
                    labelWidth:80,
                    items: [
                        { boxLabel: '庫別', width: 45, name: 'condition1', inputValue: '0', checked: true },
                        { boxLabel: '庫別類別', width: 70, name: 'condition1', inputValue: '1' },
                    ],
                    listeners: {
                        change: function (field, newValue, oldValue) {
                            changeCondition1(newValue['condition1']);
                        }
                    },
                },
                {
                    xtype: 'combo',
                    store: whnoQueryStore,
                    name: 'P0',
                    id: 'P0',
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    width: 190,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'combo',
                    store: whgradeQueryStore,
                    name: 'P1',
                    id: 'P1',
                    displayField: 'GRADE_NAME',
                    valueField: 'GRADE_VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    width: 80,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{GRADE_NAME}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'radiogroup',
                    name: 'reportType',
                    fieldLabel: '報表類別',
                    labelWidth: 80,
                    items: [
                        { boxLabel: '全部', width: 45, name: 'reportType', inputValue: 'all', checked: true },
                        { boxLabel: '入帳', width: 45, name: 'reportType', inputValue: 'in' },
                        { boxLabel: '出帳', width: 45, name: 'reportType', inputValue: 'out' },
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢', handler: function () {
                        var f = T1Query.getForm();
                        // 查詢條件1
                        if (f.findField('condition1').getValue()['condition1'] == 0 && 
                            !f.findField('P0').getValue()) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>查詢條件</span>未完整輸入');
                            return;
                        }
                        if (f.findField('condition1').getValue()['condition1'] == 1 &&
                            !f.findField('P1').getValue()) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>查詢條件</span>未完整輸入');
                            return;
                        }
                        // 查詢條件2
                        if (f.findField('condition2').getValue()['condition2'] == 0 &&
                            (!f.findField('P2').getValue() || 
                             !f.findField('P3').getValue())) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>查詢條件</span>未完整輸入');
                            return;
                        }
                        if (f.findField('condition2').getValue()['condition2'] == 1 &&
                            !f.findField('P4').getValue()) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>查詢條件</span>未完整輸入');
                            return;
                        }
                        
                        var queryItem = {
                            condition1: f.findField('condition1').getValue()['condition1'],
                            p0: f.findField('P0').getValue(),
                            p1: f.findField('P1').getValue(),
                            condition2: f.findField('condition2').getValue()['condition2'],
                            p2 : f.findField('P2').getValue(),
                            p3: f.findField('P3').getValue(),
                            p4: f.findField('P4').getValue(),
                            reportType: f.findField('reportType').getValue()['reportType']
                        };

                        reportAutoGenerate(true, queryItem);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        msglabel('');
                    }
                }
            ]
        },
            {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'radiogroup',
                        name: 'condition2',
                        fieldLabel: '查詢條件2',
                        labelWidth: 80,
                        items: [
                            { boxLabel: '查詢日期', width: 70, name: 'condition2', inputValue: '0', checked: true },
                            { boxLabel: '月結', width: 45, name: 'condition2', inputValue: '1' },
                        ],
                        listeners: {
                            change: function (field, newValue, oldValue) {
                                changeCondition2(newValue['condition2']);
                            }
                        },
                    },
                    {
                        xtype: 'datefield',
                        name: 'P2',
                        id: 'P2',
                        width: 80,
                        fieldCls: 'required'
                    },
                    {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        name: 'P3',
                        id: 'P3',
                        labelSeparator: '',
                        width: 90,
                        labelWidth: 10,
                        fieldCls: 'required'
                    },
                    {
                        xtype: 'monthfield',
                        name: 'P4',
                        id: 'P4',
                        width: 70,
                        format: 'Xm',
                        fieldCls: 'required'
                    }
                ]
            }
        ]
    });

    function changeCondition1(condition1){
        if (condition1 == '0') {    // 庫房
            T1Query.getForm().findField('P0').setValue('');
            T1Query.getForm().findField('P1').setValue('');
            T1Query.getForm().findField('P0').show();
            T1Query.getForm().findField('P1').hide();
        } else {                    // 庫房類別
            T1Query.getForm().findField('P0').setValue('');
            T1Query.getForm().findField('P1').setValue('');
            T1Query.getForm().findField('P0').hide();
            T1Query.getForm().findField('P1').show();
        }
    }
    function changeCondition2(condition2) {
        if (condition2 == '0') {    // 查詢日期
            T1Query.getForm().findField('P2').setValue('');
            T1Query.getForm().findField('P3').setValue('');
            T1Query.getForm().findField('P4').setValue('');
            T1Query.getForm().findField('P2').show();
            T1Query.getForm().findField('P3').show();
            T1Query.getForm().findField('P4').hide();
        } else {                    // 月結
            T1Query.getForm().findField('P2').setValue('');
            T1Query.getForm().findField('P3').setValue('');
            T1Query.getForm().findField('P4').setValue('');
            T1Query.getForm().findField('P2').hide();
            T1Query.getForm().findField('P3').hide();
            T1Query.getForm().findField('P4').show();
        }
    }

    var T1Store = Ext.create('WEBAPP.store.BcItmanager', { // 定義於/Scripts/app/store/BcItmanager.js 
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records) {
                
                if (records.length > 0) {
                    T1Grid.down('#update').setDisabled(false);
                    T1Grid.down('#cancel').setDisabled(false);
                } else {
                    T1Grid.down('#update').setDisabled(true);
                    T1Grid.down('#cancel').setDisabled(true);
                }
            }
        },

    });
    function T1Load(clearMsg) {
        T1Tool.moveFirst();
        if (clearMsg) {
            msglabel('訊息區:');
        }
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1print', text: '列印', handler: function () {
                    if (Ext.getCmp('radio1').getValue() == true) {
                        T1Query.getForm().findField('P2').setValue(Ext.getCmp('radio1').inputValue);
                    } else {
                        T1Query.getForm().findField('P2').setValue(Ext.getCmp('radio2').inputValue);
                    }

                    showReport();
                }
            }]
    });

    //function showReport() {
    //    if (!win) {
    //        var winform = Ext.create('Ext.form.Panel', {
    //            id: 'iframeReport',
    //            //height: '100%',
    //            //width: '100%',
    //            layout: 'fit',
    //            closable: false,
    //            html: '<iframe src="' + reportUrl + '?STARTDATE=' + getDateString(T1Query.getForm().findField('P0').getValue())
    //            + '&ENDDATE=' + getDateString(T1Query.getForm().findField('P1').getValue())
    //            + '&PRINTTYPE=' + T1Query.getForm().findField('P2').getValue()
    //            + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
    //            buttons: [{
    //                text: '關閉',
    //                handler: function () {
    //                    this.up('window').destroy();
    //                }
    //            }]
    //        });
    //        var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
    //    }
    //    win.show();
    //}

    function reportAutoGenerate(autoGen, queryItem) {
        if (autoGen) {
            

            queryItem.p0 = queryItem.p0 == null ? '' : queryItem.p0;
            queryItem.p1 = queryItem.p1 == null ? '' : queryItem.p1;
            queryItem.p2 = getDateString(T1Query.getForm().findField('P2').getValue());
            queryItem.p3 = getDateString(T1Query.getForm().findField('P3').getValue());
            queryItem.p4 = getDateString(T1Query.getForm().findField('P4').getValue());

            var qstring = '';
            qstring += 'CONDITION1=' + queryItem.condition1;
            qstring += '&WHNO=' + queryItem.p0;
            qstring += '&WHGRADE=' + queryItem.p1;
            qstring += '&CONDITION2=' + queryItem.condition2;
            qstring += '&STARTDATE=' + queryItem.p2;
            qstring += '&ENDDATE=' + queryItem.p3;
            qstring += '&SELECTMONTH=' + queryItem.p4;
            qstring += '&REPORTTYPE=' + queryItem.reportType;
            //qstring += + 'STARTDATE=' + p0 + '&ENDDATE=' + p1 + '&PRINTTYPE=' + p2;


            Ext.getDom('mainContent').src = autoGen ? reportUrl + '?' + qstring : '';
        }
    };

    function getDateString(date) {

        if (date == null || date == '') {
            return '';
        }

        var y = date.getFullYear();
        var m = (date.getMonth() + 1).toString();
        var d = (date.getDate()).toString();
        var mm = m.length > 1 ? m : "0" + m;
        var dd = d.length > 1 ? d : "0" + d;
        return y + "-" + mm + "-" + dd;
    }

    
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 'tabReport',
                region: 'north',
                frame: false,
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Query]
            },
            {
                region: 'center',
                xtype: 'form',
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>'
            }
        ]
    });
    
    reportAutoGenerate(false, null);
    //setPrintType();
    changeCondition1('0');
    changeCondition2('0');

    T1Query.getForm().findField('P0').focus();
});

Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.DocButton']);

Ext.onReady(function () {
    // var T1Get = '/api/AA0097/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "新合約品項批次更新";

    var T1Rec = 0;
    var T1LastRec = null;
    var IsSend = false;

    var T1GetExcel = '../../../api/AA0097/Excel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    Ext.define('MyApp.view.component.Number', {
        override: 'Ext.form.field.Number',
        forcePrecision: false,

        // changing behavior of valueToRaw with forcePrecision
        valueToRaw: function (value) {
            var me = this,
                decimalSeparator = me.decimalSeparator;

            value = me.parseValue(value); // parses value received from the field
            value = me.fixPrecision(value); // applies precision
            value = Ext.isNumber(value) ? value :
                parseFloat(String(value).replace(decimalSeparator, '.'));
            value = isNaN(value) ? '' : String(value).replace('.', decimalSeparator);

            // forcePrecision: true - retains a precision
            // forcePrecision: false - does not retain precision for whole numbers
            if (value == "") {
                return "";

            }
            else {
                return me.forcePrecision ? Ext.Number.toFixed(
                    parseFloat(value),
                    me.decimalPrecision)
                    :
                    parseFloat(
                        Ext.Number.toFixed(
                            parseFloat(value),
                            me.decimalPrecision
                        )
                    );
            }
        }
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0097VM', { // 定義於/Scripts/app/store/AA/AA0097VM.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {

                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
    }

    function T1Load() {
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [          
            {
                xtype: 'docbutton',
                text: '下載範本',
                documentKey: 'AA0097',
                hidden: true
            },
            {
                id: 't1export', text: '匯出', disabled: false, handler: function () {
                    var today = getTodayDate();
                    var p = new Array();
                    //p.push({ name: 'FN', value: today + '_新合約品項批次更新_物料主檔.xls' });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                xtype: 'filefield',
                name: 'send',
                id: 'send',
                buttonOnly: true,
                buttonText: '選擇檔案匯入',
                width: 90,
                listeners: {
                    change: function (widget, value, eOpts) {
                        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        Ext.getCmp('insert').setDisabled(true);
                        T1Store.removeAll();
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('send').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        }
                        else {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AA0097/SendExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    if (!data.success) {
                                        T1Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('insert').setDisabled(true);
                                        IsSend = false;
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        T1Store.loadData(data.etts, false);
                                        IsSend = true;                                        
                                        T1Grid.columns[1].setVisible(true);
                                        T1Grid.columns[2].setVisible(true);
                                        if (data.msg == "True") {
                                            Ext.getCmp('insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。");      
                                        };
                                        if (data.msg == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                        };
                                    }
                                    Ext.getCmp('send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('insert').setDisabled(true);
                                    myMask.hide();

                                }
                            });
                        }
                    }
                }
            },
            {
                xtype: 'button',
                text: '確認更新物料主檔',
                id: 'insert',
                name: 'insert',
                disabled: true,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AA0097/Insert',
                        method: reqVal_p,
                        params: {
                            data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                //T1Store.loadData(data.etts, false);
                                if (data.msg == "True") {
                                    Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                    msglabel("訊息區:<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                    Ext.getCmp('insert').setDisabled(true);
                                }
                                else {
                                    Ext.MessageBox.alert("提示", "主檔更新<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                    msglabel("訊息區:主檔更新<span style=\"color: red; font-weight: bold\">完成</span>");
                                    Ext.getCmp('insert').setDisabled(true);
                                }
                            }
                            myMask.hide();
                        },
                        failure: function (form, action) {
                            myMask.hide();
                            switch (action.failureType) {
                                case Ext.form.action.Action.CLIENT_INVALID:
                                    Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                    break;
                                case Ext.form.action.Action.CONNECT_FAILURE:
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    break;
                                case Ext.form.action.Action.SERVER_INVALID:
                                    Ext.Msg.alert('失敗', "匯入失敗");
                                    break;
                            }
                        }
                    });
                }
            },
        ]
    });

    //查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        // title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }],
        columns: [
            {
                xtype: 'rownumberer',
                width: 35,
                align: 'Center',
                labelAlign: 'Center'
            },
            {
                text: "檢核結果",
                dataIndex: 'CHECK_RESULT',
                hidden: true,
                width: 250
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 300
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 300
            },
            {
                text: "庫備識別碼(0非庫備,1庫備)",
                dataIndex: 'M_STOREID',
                renderer: function (value) {
                    switch (value) {
                        case '0': return '0 非庫備'; break;
                        case '1': return '1 庫備'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            {
                text: "庫備識別碼(0非庫備,1庫備)(新)",
                dataIndex: 'M_STOREID_N',
                renderer: function (value) {
                    switch (value) {
                        case '0': return '0 非庫備'; break;
                        case '1': return '1 庫備'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            {
                text: "優惠最小單價(計量單位單價)",
                dataIndex: 'DISC_UPRICE',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "優惠最小單價(計量單位單價)(新)",
                dataIndex: 'DISC_UPRICE_N',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "申購計量單位(包裝單位)",
                dataIndex: 'M_PURUN',
                width: 200
            },
            {
                text: "申購計量單位(包裝單位)(新)",
                dataIndex: 'M_PURUN_N',
                width: 200
            },
            {
                text: "包裝轉換率",
                dataIndex: 'EXCH_RATIO',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "包裝轉換率(新)",
                dataIndex: 'EXCH_RATIO_N',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "長度(CM)",
                dataIndex: 'M_VOLL',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "長度(CM)(新)",
                dataIndex: 'M_VOLL_N',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "寬度(CM)",
                dataIndex: 'M_VOLW',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "寬度(CM)(新)",
                dataIndex: 'M_VOLW_N',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "高度(CM)",
                dataIndex: 'M_VOLH',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "高度(CM)(新)",
                dataIndex: 'M_VOLH_N',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "圓周(CM)",
                dataIndex: 'M_VOLC',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "圓周(CM)(新)",
                dataIndex: 'M_VOLC_N',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "材積轉換率",
                dataIndex: 'M_SWAP',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "材積轉換率(新)",
                dataIndex: 'M_SWAP_N',
                align: 'right',
                style: 'text-align:left',
                format: '0.00',
                xtype: 'numbercolumn',
                width: 200
            },
            {
                text: "ID碼",
                dataIndex: 'M_IDKEY',
                width: 200
            },
            {
                text: "ID碼(新)",
                dataIndex: 'M_IDKEY_N',
                width: 200
            },
            {
                text: "衛材料號碼",
                dataIndex: 'M_INVKEY',
                width: 200
            },
            {
                text: "衛材料號碼(新)",
                dataIndex: 'M_INVKEY_N',
                width: 200
            },
            {
                text: "行政院碼",
                dataIndex: 'M_GOVKEY',
                width: 200
            },
            {
                text: "行政院碼(新)",
                dataIndex: 'M_GOVKEY_N',
                width: 200
            },
            {
                text: "消耗屬性(1消耗,2半消耗)",
                dataIndex: 'M_CONSUMID',
                renderer: function (value) {
                    switch (value) {
                        case '1': return '1 消耗'; break;
                        case '2': return '2 半消耗'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            {
                text: "消耗屬性(1消耗,2半消耗)(新)",
                dataIndex: 'M_CONSUMID_N',
                renderer: function (value) {
                    switch (value) {
                        case '1': return '1 消耗'; break;
                        case '2': return '2 半消耗'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            {
                text: "給付類別(1自費,2健保,3醫院吸收)",
                dataIndex: 'M_PAYKIND',
                renderer: function (value) {
                    switch (value) {
                        case '1': return '1 自費'; break;
                        case '2': return '2 健保'; break;
                        case '3': return '3 醫院吸收'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            {
                text: "給付類別(1自費,2健保,3醫院吸收)(新)",
                dataIndex: 'M_PAYKIND_N',
                renderer: function (value) {
                    switch (value) {
                        case '1': return '1 自費'; break;
                        case '2': return '2 健保'; break;
                        case '3': return '3 醫院吸收'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            {
                text: "計費方式(1計價,2不計價)",
                dataIndex: 'M_PAYID',
                renderer: function (value) {
                    switch (value) {
                        case '1': return '1 計價'; break;
                        case '2': return '2 不計價'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            {
                text: "計費方式(1計價,2不計價)(新)",
                dataIndex: 'M_PAYID_N',
                renderer: function (value) {
                    switch (value) {
                        case '1': return '1 計價'; break;
                        case '2': return '2 不計價'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            {
                text: "扣庫方式(1扣庫,2不扣庫)",
                dataIndex: 'M_TRNID',
                renderer: function (value) {
                    switch (value) {
                        case '1': return '1 扣庫'; break;
                        case '2': return '2 不扣庫'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            {
                text: "扣庫方式(1扣庫,2不扣庫)(新)",
                dataIndex: 'M_TRNID_N',
                renderer: function (value) {
                    switch (value) {
                        case '1': return '1 扣庫'; break;
                        case '2': return '2 不扣庫'; break;
                        default: return value; break;
                    }
                },
                width: 200
            },
            //{
            //    text: "是否盤盈虧(Y盤盈虧,N消耗)",
            //    dataIndex: 'INV_POSTID',
            //    renderer: function (value) {
            //        switch (value) {
            //            case 'Y': return 'Y 盤盈虧'; break;
            //            case 'N': return 'N 消耗'; break;
            //            default: return value; break;
            //        }
            //    },
            //    width: 200
            //},
            //{
            //    text: "是否盤盈虧(Y盤盈虧,N消耗)(新)",
            //    dataIndex: 'INV_POSTID_N',
            //    renderer: function (value) {
            //        switch (value) {
            //            case 'Y': return 'Y 盤盈虧'; break;
            //            case 'N': return 'N 消耗'; break;
            //            default: return value; break;
            //        }
            //    },
            //    width: 200
            //},
        ],
        listeners: {
            selectionchange: function (model, records) {

            }
        }
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        ]
    });   
});
